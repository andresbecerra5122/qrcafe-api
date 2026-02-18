using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Dto.WaiterCalls;
using QrCafe.Application.WaiterCalls.Commands.AttendWaiterCall;
using QrCafe.Application.WaiterCalls.Queries.GetWaiterCalls;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/waiter-calls")]
    public class OpsWaiterCallsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OpsWaiterCallsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<WaiterCallDto>>> Get(
            [FromQuery] Guid restaurantId,
            [FromQuery] string? status,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetWaiterCallsQuery(restaurantId, status), ct);
            var dto = result.Items.Select(i => new WaiterCallDto(
                i.Id, i.TableNumber, i.Status, i.CreatedAt, i.AttendedAt
            )).ToList();

            return Ok(dto);
        }

        [HttpPatch("{callId:guid}/attend")]
        public async Task<IActionResult> Attend(Guid callId, CancellationToken ct)
        {
            await _mediator.Send(new AttendWaiterCallCommand(callId), ct);
            return NoContent();
        }
    }
}
