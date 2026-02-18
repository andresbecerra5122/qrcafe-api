using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Dto.Tables;
using QrCafe.Api.Mappers;
using QrCafe.Application.Tables.Queries.ResolveTable;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("public/restaurants/{restaurantId:guid}/tables")]
    public class PublicTablesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicTablesController(IMediator mediator) => _mediator = mediator;

        [HttpGet("resolve")]
        public async Task<ActionResult<TablePublicDto>> Resolve(
            Guid restaurantId,
            [FromQuery] string number,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new ResolveTableQuery(restaurantId, number), ct);
            if (result is null) return NotFound();
            return Ok(result.ToDto());
        }
    }
}
