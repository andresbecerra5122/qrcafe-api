using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.UpdateOrderItemPrepared
{
    public class UpdateOrderItemPreparedHandler : IRequestHandler<UpdateOrderItemPreparedCommand>
    {
        private readonly QrCafeDbContext _db;
        public UpdateOrderItemPreparedHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(UpdateOrderItemPreparedCommand request, CancellationToken ct)
        {
            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct)
                ?? throw new KeyNotFoundException("Order not found.");

            if (order.Status is OrderStatus.PAID or OrderStatus.CANCELLED)
                throw new InvalidOperationException("Cannot modify items for a closed order.");

            var item = await _db.OrderItems
                .SingleOrDefaultAsync(i => i.Id == request.OrderItemId && i.OrderId == request.OrderId, ct)
                ?? throw new KeyNotFoundException("Order item not found.");

            item.IsPrepared = request.IsPrepared;
            item.IsDone = request.IsPrepared;
            if (!request.IsPrepared)
            {
                item.IsDelivered = false;
            }

            // Persist item changes first, then derive the order status from current DB state.
            // This avoids stale status writes when multiple items are toggled concurrently.
            await _db.SaveChangesAsync(ct);

            var statusOrder = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct)
                ?? throw new KeyNotFoundException("Order not found.");

            if (statusOrder.Status is not (OrderStatus.PAID or OrderStatus.CANCELLED))
            {
                var anyItems = await _db.OrderItems
                    .Where(i => i.OrderId == request.OrderId)
                    .AnyAsync(ct);

                var allPrepared = anyItems && await _db.OrderItems
                    .Where(i => i.OrderId == request.OrderId)
                    .AllAsync(i => i.IsPrepared, ct);

                var anyPrepared = await _db.OrderItems
                    .Where(i => i.OrderId == request.OrderId)
                    .AnyAsync(i => i.IsPrepared, ct);

                var nextStatus = statusOrder.Status;
                if (allPrepared)
                {
                    nextStatus = OrderStatus.READY;
                }
                else if (anyPrepared)
                {
                    nextStatus = OrderStatus.IN_PROGRESS;
                }
                else
                {
                    nextStatus = OrderStatus.CREATED;
                }

                if (statusOrder.Status != nextStatus)
                {
                    statusOrder.Status = nextStatus;
                    statusOrder.UpdatedAt = DateTimeOffset.UtcNow;
                    await _db.SaveChangesAsync(ct);
                }
            }
        }
    }
}
