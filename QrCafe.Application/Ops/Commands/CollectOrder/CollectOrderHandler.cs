using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.CollectOrder
{
    public class CollectOrderHandler : IRequestHandler<CollectOrderCommand>
    {
        private readonly QrCafeDbContext _db;
        public CollectOrderHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(CollectOrderCommand request, CancellationToken ct)
        {
            var order = await _db.Orders
                .SingleOrDefaultAsync(o => o.Id == request.OrderId, ct)
                ?? throw new KeyNotFoundException("Order not found.");

            if (order.Status != OrderStatus.DELIVERED && order.Status != OrderStatus.PAYMENT_PENDING)
                throw new InvalidOperationException("Order must be DELIVERED or PAYMENT_PENDING to collect.");

            var requestedCode = request.PaymentMethod?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(requestedCode))
                throw new ArgumentException("Payment method is required.");

            var configuredMethod = await _db.RestaurantPaymentMethods.AsNoTracking()
                .Where(m => m.RestaurantId == order.RestaurantId && m.IsActive && m.Code == requestedCode)
                .Select(m => new { m.Code, m.Label })
                .SingleOrDefaultAsync(ct);
            if (configuredMethod is null)
                throw new ArgumentException("Invalid payment method for this restaurant.");

            var now = DateTimeOffset.UtcNow;
            order.PaymentMethod = Enum.TryParse<PaymentMethod>(configuredMethod.Code, true, out var method)
                ? method
                : null;
            order.PaymentMethodLabel = configuredMethod.Label;
            order.PaymentRequestedAt = now;
            order.PaidAt = now;
            order.Status = OrderStatus.PAID;
            order.UpdatedAt = now;

            await _db.SaveChangesAsync(ct);
        }
    }
}
