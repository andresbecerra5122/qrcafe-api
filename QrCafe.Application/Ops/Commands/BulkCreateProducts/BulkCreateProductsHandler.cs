using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.BulkCreateProducts
{
    public class BulkCreateProductsHandler : IRequestHandler<BulkCreateProductsCommand, BulkCreateProductsResult>
    {
        private readonly QrCafeDbContext _db;

        public BulkCreateProductsHandler(QrCafeDbContext db)
        {
            _db = db;
        }

        public async Task<BulkCreateProductsResult> Handle(BulkCreateProductsCommand request, CancellationToken ct)
        {
            if (request.Products is null || request.Products.Count == 0)
            {
                throw new ArgumentException("At least one product is required.");
            }

            var categoryCache = await _db.Categories
                .Where(c => c.RestaurantId == request.RestaurantId)
                .ToDictionaryAsync(c => c.Name.Trim().ToLower(), c => c, ct);

            var now = DateTimeOffset.UtcNow;
            var nextCategorySort = await _db.Categories
                .Where(c => c.RestaurantId == request.RestaurantId)
                .Select(c => (int?)c.Sort)
                .MaxAsync(ct) ?? 0;

            var productsToCreate = new List<Product>(request.Products.Count);
            foreach (var item in request.Products)
            {
                var name = item.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Product name is required.");
                }

                if (item.Price < 0)
                {
                    throw new ArgumentException($"Price must be greater than or equal to zero. Product: {name}");
                }

                var categoryId = ResolveCategoryId(
                    request.RestaurantId,
                    item.CategoryName,
                    categoryCache,
                    () => ++nextCategorySort,
                    now
                );

                productsToCreate.Add(new Product
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = request.RestaurantId,
                    CategoryId = categoryId,
                    Name = name,
                    Description = item.Description?.Trim(),
                    Price = item.Price,
                    IsActive = true,
                    IsAvailable = item.IsAvailable,
                    Sort = item.Sort,
                    CreatedAt = now,
                    UpdatedAt = now,
                    ImageUrl = item.ImageUrl?.Trim() ?? string.Empty,
                    PrepStation = string.IsNullOrWhiteSpace(item.PrepStation) ? null : ParsePrepStation(item.PrepStation)
                });
            }

            _db.Products.AddRange(productsToCreate);
            await _db.SaveChangesAsync(ct);

            return new BulkCreateProductsResult(productsToCreate.Count);
        }

        private Guid? ResolveCategoryId(
            Guid restaurantId,
            string? categoryName,
            Dictionary<string, Category> categoryCache,
            Func<int> getNextSort,
            DateTimeOffset now)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return null;
            }

            var normalizedName = categoryName.Trim();
            var key = normalizedName.ToLower();

            if (categoryCache.TryGetValue(key, out var existing))
            {
                return existing.Id;
            }

            var created = new Category
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                Name = normalizedName,
                Sort = getNextSort(),
                PrepStation = PrepStation.KITCHEN,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Categories.Add(created);
            categoryCache[key] = created;
            return created.Id;
        }

        private static PrepStation ParsePrepStation(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return PrepStation.KITCHEN;
            }

            if (!Enum.TryParse<PrepStation>(raw, true, out var station))
            {
                throw new ArgumentException("Invalid prepStation. Use KITCHEN or BAR.");
            }

            return station;
        }
    }
}
