using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Menu.Queries.GetMenu
{
    public class GetMenuHandler : IRequestHandler<GetMenuQuery, GetMenuResult?>
    {
        private readonly QrCafeDbContext _db;
        public GetMenuHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetMenuResult?> Handle(GetMenuQuery request, CancellationToken ct)
        {
            var exists = await _db.Restaurants.AsNoTracking()
                .AnyAsync(r => r.Id == request.RestaurantId && r.IsActive, ct);

            if (!exists) return null;

            var categories = await _db.Categories.AsNoTracking()
                .Where(c => c.RestaurantId == request.RestaurantId)
                .OrderBy(c => c.Sort).ThenBy(c => c.Name)
                .Select(c => new GetMenuCategoryItem(c.Id, c.Name, c.Sort))
                .ToListAsync(ct);

            var products = await _db.Products.AsNoTracking()
                .Where(p => p.RestaurantId == request.RestaurantId && p.IsActive)
                .OrderBy(p => p.Sort).ThenBy(p => p.Name)
                .Select(p => new GetMenuProductItem(
                    p.Id, p.CategoryId, p.Name, p.Description, p.Price, p.IsAvailable, p.Sort, p.ImageUrl
                ))
                .ToListAsync(ct);

            var restaurant = await _db.Restaurants
                .AsNoTracking()
                .SingleAsync(r => r.Id == request.RestaurantId && r.IsActive, ct);

            return new GetMenuResult(request.RestaurantId, restaurant.Name, restaurant.Currency, categories, products);
        }
    }
}
