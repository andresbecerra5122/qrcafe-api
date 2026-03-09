using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Application.Ops.Commands.CreateRestaurantOnboarding;

namespace QrCafe.Api.Controllers.Ops
{
    [ApiController]
    [Route("ops/platform/onboarding")]
    [Authorize(Policy = AuthConstants.PolicySuperAdminOnly)]
    public class OpsPlatformOnboardingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OpsPlatformOnboardingController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
