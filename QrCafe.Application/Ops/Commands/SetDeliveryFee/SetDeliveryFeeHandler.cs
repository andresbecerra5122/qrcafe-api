using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.SetDeliveryFee
{
    public class SetDeliveryFeeHandler : IRequestHandler<SetDeliveryFeeCommand>
    {
        private readonly QrCafeDbContext _db;

        public SetDeliveryFeeHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(SetDeliveryFeeCommand request, CancellationToken ct)
        {
            if (request.DeliveryFee < 0)
                throw new ArgumentException("Delivery fee must be >= 0.");

            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct)
                ?? throw new KeyNotFoundException("Order not found.");

            if (order.OrderType != OrderType.DELIVERY)
                throw new InvalidOperationException("Delivery fee can only be set for DELIVERY orders.");

            if (order.Status is OrderStatus.PAID or OrderStatus.CANCELLED)
                throw new InvalidOperationException("Cannot edit delivery fee for paid or cancelled orders.");

            order.DeliveryFee = Math.Round(request.DeliveryFee, 2);
            order.Total = order.Subtotal + order.Tax + order.DeliveryFee + order.TipAmount;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}
