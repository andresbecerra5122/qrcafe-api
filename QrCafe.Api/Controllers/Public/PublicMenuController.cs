using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Dto.Menu;
using QrCafe.Api.Mappers;
using QrCafe.Application.Menu.Queries.GetMenu;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("public/restaurants/{restaurantId:guid}/menu")]
    public class PublicMenuController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicMenuController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<MenuDto>> Get(Guid restaurantId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMenuQuery(restaurantId), ct);
            if (result is null) return NotFound();

            return Ok(MenuMapper.ToDto(result));
        }
    }
}
