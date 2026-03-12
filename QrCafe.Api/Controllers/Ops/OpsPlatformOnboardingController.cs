using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Application.Ops.Commands.CreateRestaurantOnboarding;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Ops
{
    [ApiController]
    [Route("ops/platform/onboarding")]
    [Authorize(Policy = AuthConstants.PolicySuperAdminOnly)]
    public class OpsPlatformOnboardingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly QrCafeDbContext _db;

        public OpsPlatformOnboardingController(IMediator mediator, QrCafeDbContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [HttpPost("restaurants")]
        public async Task<ActionResult<CreateRestaurantOnboardingResponseDto>> CreateRestaurant(
            [FromBody] CreateRestaurantOnboardingRequestDto req,
            CancellationToken ct)
        {
            var command = new CreateRestaurantOnboardingCommand(
                new CreateRestaurantOnboardingInput(
                    req.Name,
                    req.Slug,
                    req.CountryCode,
                    req.Currency,
                    req.TimeZone,
                    req.TaxRate,
                    req.EnableDineIn,
                    req.EnableDelivery,
                    req.EnableDeliveryCash,
                    req.EnableDeliveryCard,
                    req.AdminFullName,
                    req.AdminEmail,
                    req.AdminPassword,
                    req.InitialTablesCount,
                    req.StaffUsers.Select(s => new CreateRestaurantOnboardingStaffInput(
                        s.FullName,
                        s.Email,
                        s.Password,
                        s.Role
                    )).ToList(),
                    req.Categories.Select(c => new CreateRestaurantOnboardingCategoryInput(
                        c.Name,
                        c.Sort,
                        c.Products.Select(p => new CreateRestaurantOnboardingProductInput(
                            p.Name,
                            p.Description,
                            p.Price,
                            p.IsActive,
                            p.IsAvailable,
                            p.Sort,
                            p.ImageUrl
                        )).ToList()
                    )).ToList()
                )
            );

            var result = await _mediator.Send(command, ct);
            return Ok(new CreateRestaurantOnboardingResponseDto
            {
                RestaurantId = result.RestaurantId,
                Name = result.Name,
                Slug = result.Slug,
                AdminEmail = result.AdminEmail,
                TablesCreated = result.TablesCreated,
                CategoriesCreated = result.CategoriesCreated,
                ProductsCreated = result.ProductsCreated,
                StaffUsersCreated = result.StaffUsersCreated
            });
        }

        [HttpGet("restaurants")]
        public async Task<ActionResult<IReadOnlyList<PlatformRestaurantListItemDto>>> GetRestaurants(CancellationToken ct)
        {
            var restaurants = await _db.Restaurants
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new PlatformRestaurantListItemDto
                {
                    RestaurantId = r.Id,
                    Name = r.Name,
                    Slug = r.Slug,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(ct);

            return Ok(restaurants);
        }

        [HttpPatch("restaurants/{restaurantId:guid}/status")]
        public async Task<IActionResult> UpdateRestaurantStatus(
            Guid restaurantId,
            [FromBody] UpdateRestaurantActiveStatusRequestDto req,
            CancellationToken ct)
        {
            var restaurant = await _db.Restaurants.SingleOrDefaultAsync(r => r.Id == restaurantId, ct);
            if (restaurant is null)
            {
                return NotFound(new { error = "Restaurant not found." });
            }

            restaurant.IsActive = req.IsActive;
            restaurant.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
