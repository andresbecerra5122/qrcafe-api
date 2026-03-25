using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Orders.Commands.RequestPayment
{
    public class RequestPaymentHandler : IRequestHandler<RequestPaymentCommand>
    {
        private readonly QrCafeDbContext _db;
        public RequestPaymentHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(RequestPaymentCommand request, CancellationToken ct)
        {
            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct)
                ?? throw new KeyNotFoundException("Order not found.");

            if (order.Status != OrderStatus.DELIVERED)
                throw new ArgumentException($"Order must be DELIVERED to request payment. Current: {order.Status}");

            var requestedCode = request.PaymentMethod?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(requestedCode))
                throw new ArgumentException("Payment method is required.");

            var configuredMethod = await _db.RestaurantPaymentMethods.AsNoTracking()
                .Where(m => m.RestaurantId == order.RestaurantId && m.IsActive && m.Code == requestedCode)
                .Select(m => new { m.Code, m.Label })
                .SingleOrDefaultAsync(ct);
            if (configuredMethod is null)
                throw new ArgumentException("Invalid payment method for this restaurant.");

            order.Status = OrderStatus.PAYMENT_PENDING;
            order.PaymentMethod = Enum.TryParse<PaymentMethod>(configuredMethod.Code, true, out var method)
                ? method
                : null;
            order.PaymentMethodLabel = configuredMethod.Label;
            order.PaymentRequestedAt = DateTimeOffset.UtcNow;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}
