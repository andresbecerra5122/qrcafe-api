using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Application.Ops.Commands.ToggleProductAvailability;
using QrCafe.Application.Ops.Queries.GetOpsProducts;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/products")]
    public class OpsProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OpsProductsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OpsProductItem>>> Get(
            [FromQuery] Guid restaurantId,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOpsProductsQuery(restaurantId), ct);
            return Ok(result.Items);
        }

        [HttpPatch("{productId:guid}/availability")]
        public async Task<IActionResult> ToggleAvailability(
            Guid productId,
            [FromBody] ToggleAvailabilityDto req,
            CancellationToken ct)
        {
            await _mediator.Send(new ToggleProductAvailabilityCommand(productId, req.IsAvailable), ct);
            return NoContent();
        }
    }

    public record ToggleAvailabilityDto(bool IsAvailable);
}
