using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Restaurants.Queries.GetRestaurantBySlug
{
    public class GetRestaurantBySlugHandler : IRequestHandler<GetRestaurantBySlugQuery, GetRestaurantBySlugResult?>
    {
        private readonly QrCafeDbContext _db;

        public GetRestaurantBySlugHandler(QrCafeDbContext db)
        {
            _db = db;
        }

        public async Task<GetRestaurantBySlugResult?> Handle(GetRestaurantBySlugQuery request, CancellationToken ct)
        {
            var slug = request.Slug.Trim();

            return await _db.Restaurants
                .AsNoTracking()
                .Where(r => r.Slug == slug && r.IsActive)
                .Select(r => new GetRestaurantBySlugResult(
                    r.Id,
                    r.Name,
                    r.Slug,
                    r.CountryCode,
                    r.Currency,
                    r.TimeZone,
                    r.TaxRate
                ))
                .SingleOrDefaultAsync(ct);
        }
    }
}
