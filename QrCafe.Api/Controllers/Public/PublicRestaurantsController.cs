using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Dto.Restaurants;
using QrCafe.Application.Restaurants.Queries.GetRestaurantBySlug;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("public/restaurants")]
    public class PublicRestaurantsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicRestaurantsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("{slug}")]
        public async Task<ActionResult<RestaurantPublicDto>> GetBySlug(string slug, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetRestaurantBySlugQuery(slug), ct);
            if (result is null) return NotFound();

            var dto = new RestaurantPublicDto(
                result.Id, result.Name, result.Slug, result.CountryCode, result.Currency, result.TimeZone, result.TaxRate
            );

            return Ok(dto);
        }
    }
}
