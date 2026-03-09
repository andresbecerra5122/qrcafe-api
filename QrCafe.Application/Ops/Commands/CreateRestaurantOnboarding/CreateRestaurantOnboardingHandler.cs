using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace QrCafe.Application.Ops.Commands.CreateRestaurantOnboarding
{
    public class CreateRestaurantOnboardingHandler : IRequestHandler<CreateRestaurantOnboardingCommand, CreateRestaurantOnboardingResult>
    {
        private static readonly Regex SlugCleaner = new("[^a-z0-9-]", RegexOptions.Compiled);
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private readonly QrCafeDbContext _db;

        public CreateRestaurantOnboardingHandler(QrCafeDbContext db)
        {
            _db = db;
        }

        public async Task<CreateRestaurantOnboardingResult> Handle(CreateRestaurantOnboardingCommand request, CancellationToken ct)
        {
            var req = request.Input;
            var restaurantName = req.Name.Trim();
            var slug = NormalizeSlug(req.Slug);
            var adminFullName = req.AdminFullName.Trim();
            var adminEmail = req.AdminEmail.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(restaurantName)
                || string.IsNullOrWhiteSpace(slug)
                || string.IsNullOrWhiteSpace(adminFullName)
                || string.IsNullOrWhiteSpace(adminEmail)
                || string.IsNullOrWhiteSpace(req.AdminPassword))
            {
                throw new ArgumentException("name, slug, adminFullName, adminEmail and adminPassword are required.");
            }

            if (req.TaxRate < 0 || req.TaxRate > 1)
            {
                throw new ArgumentException("taxRate must be between 0 and 1.");
            }

            if (req.InitialTablesCount < 0 || req.InitialTablesCount > 200)
            {
                throw new ArgumentException("initialTablesCount must be between 0 and 200.");
            }

            var slugExists = await _db.Restaurants.AsNoTracking().AnyAsync(r => r.Slug.ToLower() == slug, ct);
            if (slugExists)
            {
                throw new ArgumentException("A restaurant with this slug already exists.");
            }

            var now = DateTimeOffset.UtcNow;
            var restaurantId = Guid.NewGuid();
            var normalizedStaff = new List<(string FullName, string Email, string Password, StaffRole Role)>();
            var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { adminEmail };

            foreach (var staff in req.StaffUsers)
            {
                var fullName = staff.FullName.Trim();
                var email = staff.Email.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(staff.Password))
                {
                    throw new ArgumentException("Each staff user requires fullName, email and password.");
                }

                if (!Enum.TryParse<StaffRole>(staff.Role, true, out var role))
                {
                    throw new ArgumentException($"Invalid staff role: {staff.Role}");
                }

                if (!emails.Add(email))
                {
                    throw new ArgumentException($"Duplicate email in request: {email}");
                }

                normalizedStaff.Add((fullName, email, staff.Password, role));
            }

            await using var trx = await _db.Database.BeginTransactionAsync(ct);

            var restaurant = new Restaurant
            {
                Id = restaurantId,
                Name = restaurantName,
                Slug = slug,
                CountryCode = req.CountryCode.Trim(),
                Currency = req.Currency.Trim(),
                TimeZone = req.TimeZone.Trim(),
                TaxRate = req.TaxRate,
                IsActive = true,
                EnableDineIn = req.EnableDineIn,
                EnableDelivery = req.EnableDelivery,
                EnableDeliveryCash = req.EnableDeliveryCash,
                EnableDeliveryCard = req.EnableDeliveryCard,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Restaurants.Add(restaurant);
            await _db.SaveChangesAsync(ct);

            _db.StaffUsers.Add(new StaffUser
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                FullName = adminFullName,
                Email = adminEmail,
                PasswordHash = HashPassword(req.AdminPassword),
                Role = StaffRole.Admin,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            foreach (var staff in normalizedStaff)
            {
                _db.StaffUsers.Add(new StaffUser
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = restaurantId,
                    FullName = staff.FullName,
                    Email = staff.Email,
                    PasswordHash = HashPassword(staff.Password),
                    Role = staff.Role,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            var tablesCreated = 0;
            for (var i = 1; i <= req.InitialTablesCount; i++)
            {
                _db.Tables.Add(new TableEntity
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = restaurantId,
                    Number = i,
                    Token = $"mesa-{i}-{Guid.NewGuid():N}"[..24],
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                tablesCreated++;
            }

            var categoriesCreated = 0;
            var productsCreated = 0;
            for (var i = 0; i < req.Categories.Count; i++)
            {
                var catReq = req.Categories[i];
                var catName = catReq.Name.Trim();
                if (string.IsNullOrWhiteSpace(catName))
                {
                    throw new ArgumentException("Category name is required.");
                }

                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = restaurantId,
                    Name = catName,
                    Sort = catReq.Sort > 0 ? catReq.Sort : i + 1,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _db.Categories.Add(category);
                categoriesCreated++;

                for (var p = 0; p < catReq.Products.Count; p++)
                {
                    var prodReq = catReq.Products[p];
                    var productName = prodReq.Name.Trim();
                    if (string.IsNullOrWhiteSpace(productName))
                    {
                        throw new ArgumentException("Product name is required.");
                    }
                    if (prodReq.Price < 0)
                    {
                        throw new ArgumentException($"Invalid price for product: {productName}");
                    }

                    _db.Products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        RestaurantId = restaurantId,
                        CategoryId = category.Id,
                        Name = productName,
                        Description = prodReq.Description?.Trim(),
                        Price = prodReq.Price,
                        IsActive = prodReq.IsActive,
                        IsAvailable = prodReq.IsAvailable,
                        Sort = prodReq.Sort > 0 ? prodReq.Sort : p + 1,
                        ImageUrl = prodReq.ImageUrl?.Trim() ?? string.Empty,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                    productsCreated++;
                }
            }

            await _db.SaveChangesAsync(ct);
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO public.restaurant_order_counters (restaurant_id, last_number) VALUES ({restaurantId}, 0) ON CONFLICT (restaurant_id) DO NOTHING;",
                ct);
            await trx.CommitAsync(ct);

            return new CreateRestaurantOnboardingResult(
                restaurantId,
                restaurantName,
                slug,
                adminEmail,
                tablesCreated,
                categoriesCreated,
                productsCreated,
                normalizedStaff.Count + 1
            );
        }

        private static string NormalizeSlug(string slug)
        {
            var normalized = slug.Trim().ToLowerInvariant().Replace(' ', '-');
            normalized = SlugCleaner.Replace(normalized, string.Empty);
            normalized = normalized.Trim('-');
            return normalized;
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }
    }
}
