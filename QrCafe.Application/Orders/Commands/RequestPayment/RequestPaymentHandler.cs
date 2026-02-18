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

            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var method))
                throw new ArgumentException("Invalid payment method. Use CASH or CARD.");

            order.Status = OrderStatus.PAYMENT_PENDING;
            order.PaymentMethod = method;
            order.PaymentRequestedAt = DateTimeOffset.UtcNow;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}
