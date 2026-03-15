using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Application.Ops.Commands.BulkCreateProducts;
using QrCafe.Application.Ops.Commands.ToggleProductAvailability;
using QrCafe.Application.Ops.Queries.GetOpsProducts;
using QrCafe.Domain.Entities;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/products")]
    [Authorize(Policy = AuthConstants.PolicyStaffAny)]
    public class OpsProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly QrCafeDbContext _db;
        public OpsProductsController(IMediator mediator, QrCafeDbContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OpsProductItem>>> Get(
            [FromQuery] Guid restaurantId,
            [FromQuery] bool includeInactive,
            CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            if (includeInactive && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            var result = await _mediator.Send(new GetOpsProductsQuery(restaurantId, includeInactive), ct);
            return Ok(result.Items);
        }

        [HttpPatch("{productId:guid}/availability")]
        [Authorize(Policy = AuthConstants.PolicyKitchenOrAdmin)]
        public async Task<IActionResult> ToggleAvailability(
            Guid productId,
            [FromBody] ToggleAvailabilityDto req,
            CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var hasAccess = await _db.Products.AsNoTracking()
                .AnyAsync(p => p.Id == productId && p.RestaurantId == restaurantId, ct);
            if (!hasAccess)
            {
                return NotFound();
            }

            await _mediator.Send(new ToggleProductAvailabilityCommand(productId, req.IsAvailable), ct);
            return NoContent();
        }

        [HttpPost]
        [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
        public async Task<ActionResult<OpsProductItem>> Create(
            [FromBody] CreateProductRequestDto req,
            CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var name = req.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { error = "Product name is required." });
            }

            if (req.Price < 0)
            {
                return BadRequest(new { error = "Price must be greater than or equal to zero." });
            }

            var now = DateTimeOffset.UtcNow;
            var categoryId = await ResolveCategoryIdAsync(restaurantId, req.CategoryName, ct);
            var product = new Product
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                CategoryId = categoryId,
                Name = name,
                Description = req.Description?.Trim(),
                Price = req.Price,
                IsActive = true,
                IsAvailable = req.IsAvailable,
                Sort = req.Sort,
                CreatedAt = now,
                UpdatedAt = now,
                ImageUrl = req.ImageUrl?.Trim() ?? string.Empty,
                PrepStation = req.PrepStation is null ? null : ParsePrepStation(req.PrepStation)
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync(ct);

            var categoryData = await _db.Categories.AsNoTracking()
                .Where(c => c.Id == product.CategoryId)
                .Select(c => new { c.Name, c.PrepStation })
                .SingleOrDefaultAsync(ct);

            return Ok(new OpsProductItem(
                product.Id,
                product.Name,
                product.Description,
                categoryData?.Name,
                categoryData?.PrepStation.ToString() ?? PrepStation.KITCHEN.ToString(),
                product.PrepStation?.ToString() ?? categoryData?.PrepStation.ToString() ?? PrepStation.KITCHEN.ToString(),
                product.Price,
                product.IsAvailable,
                product.IsActive,
                string.IsNullOrWhiteSpace(product.ImageUrl) ? null : product.ImageUrl,
                product.Sort
            ));
        }

        [HttpPost("bulk")]
        [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
        public async Task<ActionResult<object>> BulkCreate(
            [FromBody] BulkCreateProductsRequestDto req,
            CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            if (req.Products is null || req.Products.Count == 0)
            {
                return BadRequest(new { error = "At least one product is required." });
            }

            var result = await _mediator.Send(
                new BulkCreateProductsCommand(
                    restaurantId,
                    req.Products.Select(p => new BulkCreateProductInput(
                        p.Name,
                        p.Description,
                        p.CategoryName,
                        p.PrepStation,
                        p.Price,
                        p.ImageUrl,
                        p.Sort,
                        p.IsAvailable
                    )).ToList()
                ),
                ct
            );

            return Ok(new { createdCount = result.CreatedCount });
        }

        [HttpPatch("{productId:guid}")]
        [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
        public async Task<ActionResult<OpsProductItem>> Update(
            Guid productId,
            [FromBody] UpdateProductRequestDto req,
            CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var product = await _db.Products.SingleOrDefaultAsync(
                p => p.Id == productId && p.RestaurantId == restaurantId,
                ct
            );
            if (product is null)
            {
                return NotFound();
            }

            if (req.Name is not null)
            {
                var name = req.Name.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { error = "Product name cannot be empty." });
                }

                product.Name = name;
            }

            if (req.Description is not null)
            {
                product.Description = req.Description.Trim();
            }

            if (req.Price.HasValue)
            {
                if (req.Price.Value < 0)
                {
                    return BadRequest(new { error = "Price must be greater than or equal to zero." });
                }

                product.Price = req.Price.Value;
            }

            if (req.ImageUrl is not null)
            {
                product.ImageUrl = req.ImageUrl.Trim();
            }

            if (req.Sort.HasValue)
            {
                product.Sort = req.Sort.Value;
            }

            if (req.IsAvailable.HasValue)
            {
                product.IsAvailable = req.IsAvailable.Value;
            }

            if (req.IsActive.HasValue)
            {
                product.IsActive = req.IsActive.Value;
            }

            if (req.CategoryName is not null)
            {
                product.CategoryId = await ResolveCategoryIdAsync(restaurantId, req.CategoryName, ct);
            }

            if (req.PrepStation is not null)
            {
                product.PrepStation = ParsePrepStation(req.PrepStation);
            }

            product.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            var categoryData = await _db.Categories.AsNoTracking()
                .Where(c => c.Id == product.CategoryId)
                .Select(c => new { c.Name, c.PrepStation })
                .SingleOrDefaultAsync(ct);

            return Ok(new OpsProductItem(
                product.Id,
                product.Name,
                product.Description,
                categoryData?.Name,
                categoryData?.PrepStation.ToString() ?? PrepStation.KITCHEN.ToString(),
                product.PrepStation?.ToString() ?? categoryData?.PrepStation.ToString() ?? PrepStation.KITCHEN.ToString(),
                product.Price,
                product.IsAvailable,
                product.IsActive,
                string.IsNullOrWhiteSpace(product.ImageUrl) ? null : product.ImageUrl,
                product.Sort
            ));
        }

        [HttpPatch("category-station")]
        [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
        public async Task<IActionResult> UpdateCategoryStation(
            [FromBody] UpdateCategoryStationRequestDto req,
            CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var categoryName = req.CategoryName?.Trim();
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return BadRequest(new { error = "Category name is required." });
            }

            var category = await _db.Categories.SingleOrDefaultAsync(
                c => c.RestaurantId == restaurantId && c.Name.ToLower() == categoryName.ToLower(),
                ct);
            if (category is null)
            {
                return NotFound(new { error = "Category not found." });
            }

            var station = ParsePrepStation(req.PrepStation);
            category.PrepStation = station;
            category.UpdatedAt = DateTimeOffset.UtcNow;

            var products = await _db.Products
                .Where(p => p.RestaurantId == restaurantId && p.CategoryId == category.Id)
                .ToListAsync(ct);
            foreach (var product in products)
            {
                product.PrepStation = station;
                product.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{productId:guid}")]
        [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
        public async Task<IActionResult> Delete(Guid productId, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var product = await _db.Products.SingleOrDefaultAsync(
                p => p.Id == productId && p.RestaurantId == restaurantId,
                ct
            );
            if (product is null)
            {
                return NotFound();
            }

            product.IsActive = false;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        private async Task<Guid?> ResolveCategoryIdAsync(Guid restaurantId, string? categoryName, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return null;
            }

            var normalized = categoryName.Trim();
            var existing = await _db.Categories
                .Where(c => c.RestaurantId == restaurantId)
                .FirstOrDefaultAsync(c => c.Name.ToLower() == normalized.ToLower(), ct);

            if (existing is not null)
            {
                return existing.Id;
            }

            var nextSort = await _db.Categories
                .Where(c => c.RestaurantId == restaurantId)
                .Select(c => (int?)c.Sort)
                .MaxAsync(ct) ?? 0;

            var now = DateTimeOffset.UtcNow;
            var created = new Category
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                Name = normalized,
                Sort = nextSort + 1,
                PrepStation = PrepStation.KITCHEN,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Categories.Add(created);
            await _db.SaveChangesAsync(ct);
            return created.Id;
        }

        private static PrepStation ParsePrepStation(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return PrepStation.KITCHEN;

            if (!Enum.TryParse<PrepStation>(raw, true, out var station))
                throw new ArgumentException("Invalid prepStation. Use KITCHEN or BAR.");

            return station;
        }
    }

    public record ToggleAvailabilityDto(bool IsAvailable);
    public record UpdateCategoryStationRequestDto(string CategoryName, string PrepStation);
}
