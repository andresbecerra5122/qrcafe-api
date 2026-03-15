using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.UpdateOrderItemDelivered
{
    public class UpdateOrderItemDeliveredHandler : IRequestHandler<UpdateOrderItemDeliveredCommand>
    {
        private readonly QrCafeDbContext _db;
        public UpdateOrderItemDeliveredHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(UpdateOrderItemDeliveredCommand request, CancellationToken ct)
        {
            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct)
                ?? throw new KeyNotFoundException("Order not found.");

            if (order.Status is OrderStatus.PAID or OrderStatus.CANCELLED)
                throw new InvalidOperationException("Cannot modify items for a closed order.");

            var item = await _db.OrderItems
                .SingleOrDefaultAsync(i => i.Id == request.OrderItemId && i.OrderId == request.OrderId, ct)
                ?? throw new KeyNotFoundException("Order item not found.");

            if (request.IsDelivered && !item.IsPrepared)
                throw new InvalidOperationException("Item must be prepared before marking delivered.");

            item.IsDelivered = request.IsDelivered;
            await _db.SaveChangesAsync(ct);
        }
    }
}
