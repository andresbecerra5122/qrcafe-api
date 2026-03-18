using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand>
    {
        private readonly QrCafeDbContext _db;
        public UpdateOrderStatusHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(UpdateOrderStatusCommand request, CancellationToken ct)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
                throw new ArgumentException("Invalid status.");

            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct);
            if (order is null) throw new KeyNotFoundException("Order not found.");

            if (newStatus == OrderStatus.READY)
            {
                var allPrepared = await _db.OrderItems
                    .Where(i => i.OrderId == order.Id)
                    .AllAsync(i => i.IsPrepared, ct);
                if (!allPrepared)
                    throw new InvalidOperationException("Order can only be READY when all items are prepared.");
            }

            if (newStatus == OrderStatus.DELIVERED)
            {
                var items = await _db.OrderItems
                    .Where(i => i.OrderId == order.Id)
                    .ToListAsync(ct);
                var anyDelivered = items.Any(i => i.IsDelivered);
                if (!anyDelivered)
                {
                    var preparedItems = items.Where(i => i.IsPrepared).ToList();
                    if (preparedItems.Count == 0)
                        throw new InvalidOperationException("At least one prepared item must exist before setting DELIVERED.");

                    foreach (var item in preparedItems)
                    {
                        item.IsDelivered = true;
                    }
                }
            }

            if (newStatus == OrderStatus.PAID)
            {
                var canMoveToPaid =
                    order.Status == OrderStatus.DELIVERED
                    || order.Status == OrderStatus.PAYMENT_PENDING
                    || (order.OrderType == OrderType.DELIVERY && order.Status == OrderStatus.OUT_FOR_DELIVERY);

                if (!canMoveToPaid)
                    throw new InvalidOperationException("Order must be DELIVERED, PAYMENT_PENDING or OUT_FOR_DELIVERY (delivery only) to set PAID.");

                if (!order.PaidAt.HasValue)
                {
                    order.PaidAt = DateTimeOffset.UtcNow;
                }

                if (!order.PaymentRequestedAt.HasValue)
                {
                    order.PaymentRequestedAt = order.PaidAt;
                }
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}
