using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Queries.GetRestaurantPaymentMethods
{
    public class GetRestaurantPaymentMethodsHandler : IRequestHandler<GetRestaurantPaymentMethodsQuery, IReadOnlyList<RestaurantPaymentMethodItem>>
    {
        private readonly QrCafeDbContext _db;

        public GetRestaurantPaymentMethodsHandler(QrCafeDbContext db) => _db = db;

        public async Task<IReadOnlyList<RestaurantPaymentMethodItem>> Handle(GetRestaurantPaymentMethodsQuery request, CancellationToken ct)
        {
            var methods = await _db.RestaurantPaymentMethods.AsNoTracking()
                .Where(m => m.RestaurantId == request.RestaurantId && m.IsActive)
                .OrderBy(m => m.Sort)
                .ThenBy(m => m.Label)
                .Select(m => new RestaurantPaymentMethodItem(m.Id, m.Code, m.Label, m.Sort))
                .ToListAsync(ct);

            return methods;
        }
    }
}
