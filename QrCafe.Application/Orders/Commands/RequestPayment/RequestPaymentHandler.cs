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

            var restaurantSettings = await _db.Restaurants.AsNoTracking()
                .Where(r => r.Id == order.RestaurantId)
                .Select(r => new { r.EnablePayAtCashier, r.SuggestedTipPercent })
                .SingleOrDefaultAsync(ct)
                ?? throw new KeyNotFoundException("Restaurant not found.");

            var customerChoseTip = order.OrderType == OrderType.DINE_IN && !restaurantSettings.EnablePayAtCashier;
            var (tipAmount, tipPercentApplied) = ResolveTip(
                order.Subtotal,
                restaurantSettings.SuggestedTipPercent,
                request.TipMode,
                request.TipAmount,
                customerChoseTip
            );

            order.Status = OrderStatus.PAYMENT_PENDING;
            order.PaymentMethod = Enum.TryParse<PaymentMethod>(configuredMethod.Code, true, out var method)
                ? method
                : null;
            order.PaymentMethodLabel = configuredMethod.Label;
            order.TipAmount = tipAmount;
            order.TipPercentApplied = tipPercentApplied;
            // Customer already answered tip in app (including "no tip"); waiter must not override.
            order.TipSource = customerChoseTip ? TipSource.CUSTOMER : null;
            order.Total = order.Subtotal + order.Tax + order.DeliveryFee + order.TipAmount;
            order.PaymentRequestedAt = DateTimeOffset.UtcNow;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        private static (decimal tipAmount, decimal? tipPercentApplied) ResolveTip(
            decimal subtotal,
            decimal suggestedTipPercent,
            string? rawTipMode,
            decimal? requestedTipAmount,
            bool allowTipSelection)
        {
            if (!allowTipSelection)
            {
                return (0m, null);
            }

            var tipMode = (rawTipMode ?? "NONE").Trim().ToUpperInvariant();
            if (tipMode == "SUGGESTED")
            {
                var safePercent = Math.Clamp(suggestedTipPercent, 0m, 100m);
                var amount = decimal.Round(subtotal * (safePercent / 100m), 2);
                return (amount, safePercent);
            }

            if (tipMode == "CUSTOM")
            {
                var amount = requestedTipAmount ?? throw new ArgumentException("Tip amount is required for CUSTOM tip mode.");
                if (amount < 0)
                    throw new ArgumentException("Tip amount must be >= 0.");
                return (decimal.Round(amount, 2), null);
            }

            return (0m, null);
        }
    }
}
