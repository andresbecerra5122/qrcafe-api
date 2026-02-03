using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Dto.Webhooks;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Webhooks
{
    [ApiController]
    [Route("webhooks/wompi")]
    public class WompiWebhookController : ControllerBase
    {
        private readonly QrCafeDbContext _db;
        public WompiWebhookController(QrCafeDbContext db) => _db = db;

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] WompiWebhookDto payload, CancellationToken ct)
        {
            var tx = payload?.Data?.Transaction;
            if (tx is null) return BadRequest(new { error = "Invalid payload" });

            // Nuestra referencia: "ORDER-{orderId:N}"
            if (!tx.Reference.StartsWith("ORDER-")) return Ok();

            var orderIdStr = tx.Reference["ORDER-".Length..];
            if (!Guid.TryParseExact(orderIdStr, "N", out var orderId)) return Ok();

            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == orderId, ct);
            if (order is null) return Ok();

            var payment = await _db.Payments.SingleOrDefaultAsync(p => p.OrderId == order.Id && p.Provider == PaymentProvider.WOMPI, ct);

            // Actualiza Payment y Order según status
            if (payment is not null)
            {
                payment.ProviderRef = $"{tx.Reference}|{tx.Id}";
                payment.Currency = tx.Currency;
                payment.Amount = tx.AmountInCents / 100m;
                payment.UpdatedAt = DateTimeOffset.UtcNow;
            }

            if (tx.Status.Equals("APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = OrderStatus.PAID;
                order.PaidAt = DateTimeOffset.UtcNow;
                order.UpdatedAt = DateTimeOffset.UtcNow;

                if (payment is not null) payment.Status = PaymentStatus.SUCCEEDED;
            }
            else if (tx.Status.Equals("DECLINED", StringComparison.OrdinalIgnoreCase) ||
                     tx.Status.Equals("ERROR", StringComparison.OrdinalIgnoreCase) ||
                     tx.Status.Equals("VOIDED", StringComparison.OrdinalIgnoreCase))
            {
                // MVP: orden se queda esperando pago, pero payment falla
                if (payment is not null) payment.Status = PaymentStatus.FAILED;
            }

            await _db.SaveChangesAsync(ct);
            return Ok();
        }
    }
}
