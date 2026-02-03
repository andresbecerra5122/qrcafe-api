using MediatR;
using Microsoft.Extensions.Configuration;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Domain.Entities;
using QrCafe.Infrastructure.Data;
using QrCafe.Infrastructure.Payments.Wompi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QrCafe.Application.Payments.Commands.CreateWompiCheckout
{
    public class CreateWompiCheckoutHandler : IRequestHandler<CreateWompiCheckoutCommand, CreateWompiCheckoutResult>
    {
        private readonly QrCafeDbContext _db;
        private readonly IConfiguration _cfg;

        public CreateWompiCheckoutHandler(QrCafeDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public async Task<CreateWompiCheckoutResult> Handle(CreateWompiCheckoutCommand cmd, CancellationToken ct)
        {
            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
                ?? throw new KeyNotFoundException("Order not found.");

            // ✅ Regla: solo generar checkout si la orden está pendiente de pago
            if (order.Status != OrderStatus.PAYMENT_PENDING)
                throw new ArgumentException($"Order status must be PAYMENT_PENDING. Current: {order.Status}");

            var amountInCents = (long)Math.Round(order.Total * 100m, MidpointRounding.AwayFromZero);
            var currency = order.Currency; // "COP"
            var reference = $"ORDER-{order.Id:N}";

            var publicKey = _cfg["Payments:Wompi:PublicKey"] ?? throw new Exception("Missing Payments:Wompi:PublicKey");
            var integritySecret = _cfg["Payments:Wompi:IntegritySecret"] ?? throw new Exception("Missing Payments:Wompi:IntegritySecret");
            var redirectBase = _cfg["Payments:Wompi:RedirectUrlBase"] ?? "http://localhost:4200/payment-result";

            var signature = WompiIntegrity.BuildSignature(reference, amountInCents, currency, integritySecret);
            var redirectUrl = $"{redirectBase}?orderId={order.Id}";

            // ✅ (MVP) crea/actualiza registro payment local
            var existing = await _db.Payments.SingleOrDefaultAsync(p =>
                p.OrderId == order.Id && p.Provider == PaymentProvider.WOMPI, ct);

            if (existing is null)
            {
                _db.Payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Provider = PaymentProvider.WOMPI,
                    ProviderRef = reference,        // guardamos reference
                    Status = PaymentStatus.PENDING,
                    Amount = order.Total,
                    Currency = order.Currency,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                existing.ProviderRef = reference;
                existing.Status = PaymentStatus.PENDING;
                existing.Amount = order.Total;
                existing.Currency = order.Currency;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _db.SaveChangesAsync(ct);

            return new CreateWompiCheckoutResult(publicKey, reference, amountInCents, currency, signature, redirectUrl);
        }
    }
}
