using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Dto.Ops;
using QrCafe.Api.Mappers;
using QrCafe.Application.Ops.Commands.UpdateOrderStatus;
using QrCafe.Application.Ops.Queries.GetOpsOrders;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/orders")]
    public class OpsOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OpsOrdersController(IMediator mediator) => _mediator = mediator;

        // /ops/orders?restaurantId=...&status=PAID,IN_PROGRESS,READY
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OpsOrderListItemDto>>> Get(
            [FromQuery] Guid restaurantId,
            [FromQuery] string? status,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOpsOrdersQuery(restaurantId, status), ct);
            var dto = result.Items.Select(OpsOrdersMapper.ToDto).ToList();
            return Ok(dto);
        }

        [HttpPatch("{orderId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequestDto req, CancellationToken ct)
        {
            await _mediator.Send(new UpdateOrderStatusCommand(orderId, req.Status), ct);
            return NoContent();
        }
    }
}
