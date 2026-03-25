using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Application.Common;
using QrCafe.Application.Ops.Queries.GetRestaurantPaymentMethods;
using QrCafe.Domain.Entities;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.AddRestaurantPaymentMethod
{
    public class AddRestaurantPaymentMethodHandler : IRequestHandler<AddRestaurantPaymentMethodCommand, RestaurantPaymentMethodItem>
    {
        private readonly QrCafeDbContext _db;

        public AddRestaurantPaymentMethodHandler(QrCafeDbContext db) => _db = db;

        public async Task<RestaurantPaymentMethodItem> Handle(AddRestaurantPaymentMethodCommand request, CancellationToken ct)
        {
            var label = request.Label?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Payment method label is required.");

            var activeCount = await _db.RestaurantPaymentMethods
                .CountAsync(m => m.RestaurantId == request.RestaurantId && m.IsActive, ct);
            if (activeCount >= 6)
                throw new InvalidOperationException("Maximum 6 payment methods are allowed per restaurant.");

            var baseCode = PaymentMethodCodeNormalizer.ToCode(label);
            var existingCodes = await _db.RestaurantPaymentMethods.AsNoTracking()
                .Where(m => m.RestaurantId == request.RestaurantId)
                .Select(m => m.Code)
                .ToListAsync(ct);

            var code = baseCode;
            var suffix = 2;
            while (existingCodes.Contains(code))
            {
                var suffixText = $"_{suffix}";
                var prefixLen = Math.Max(1, 30 - suffixText.Length);
                code = $"{baseCode[..Math.Min(baseCode.Length, prefixLen)].TrimEnd('_')}{suffixText}";
                suffix++;
            }

            var now = DateTimeOffset.UtcNow;
            var nextSort = activeCount + 1;
            var entity = new RestaurantPaymentMethod
            {
                Id = Guid.NewGuid(),
                RestaurantId = request.RestaurantId,
                Code = code,
                Label = label.Length <= 80 ? label : label[..80],
                IsActive = true,
                Sort = nextSort,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.RestaurantPaymentMethods.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new RestaurantPaymentMethodItem(entity.Id, entity.Code, entity.Label, entity.Sort);
        }
    }
}
