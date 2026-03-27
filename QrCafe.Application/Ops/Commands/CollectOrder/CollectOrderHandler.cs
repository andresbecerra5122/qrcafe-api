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

            var restaurantSettings = await _db.Restaurants.AsNoTracking()
                .Where(r => r.Id == order.RestaurantId)
                .Select(r => new { r.SuggestedTipPercent })
                .SingleOrDefaultAsync(ct)
                ?? throw new KeyNotFoundException("Restaurant not found.");

            if (order.TipSource != TipSource.CUSTOMER)
            {
                var (tipAmount, tipPercentApplied) = ResolveTip(
                    order.Subtotal,
                    restaurantSettings.SuggestedTipPercent,
                    request.TipMode,
                    request.TipAmount,
                    order.OrderType == OrderType.DINE_IN
                );
                order.TipAmount = tipAmount;
                order.TipPercentApplied = tipPercentApplied;
                order.TipSource = tipAmount > 0 ? TipSource.WAITER : null;
            }

            var now = DateTimeOffset.UtcNow;
            order.PaymentMethod = Enum.TryParse<PaymentMethod>(configuredMethod.Code, true, out var method)
                ? method
                : null;
            order.PaymentMethodLabel = configuredMethod.Label;
            order.Total = order.Subtotal + order.Tax + order.DeliveryFee + order.TipAmount;
            order.PaymentRequestedAt = now;
            order.PaidAt = now;
            order.Status = OrderStatus.PAID;
            order.UpdatedAt = now;

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
