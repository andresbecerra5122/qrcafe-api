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

            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var method))
                throw new ArgumentException("Invalid payment method. Use CASH or CARD.");

            var now = DateTimeOffset.UtcNow;
            order.PaymentMethod = method;
            order.PaymentRequestedAt = now;
            order.PaidAt = now;
            order.Status = OrderStatus.PAID;
            order.UpdatedAt = now;

            await _db.SaveChangesAsync(ct);
        }
    }
}
