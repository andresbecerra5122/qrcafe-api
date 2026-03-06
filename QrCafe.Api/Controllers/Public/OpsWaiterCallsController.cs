using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.WaiterCalls;
using QrCafe.Application.WaiterCalls.Commands.AttendWaiterCall;
using QrCafe.Application.WaiterCalls.Queries.GetWaiterCalls;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/waiter-calls")]
    [Authorize(Policy = AuthConstants.PolicyWaiterOrAdmin)]
    public class OpsWaiterCallsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly QrCafeDbContext _db;
        public OpsWaiterCallsController(IMediator mediator, QrCafeDbContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<WaiterCallDto>>> Get(
            [FromQuery] Guid restaurantId,
            [FromQuery] string? status,
            CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var result = await _mediator.Send(new GetWaiterCallsQuery(restaurantId, status), ct);
            var dto = result.Items.Select(i => new WaiterCallDto(
                i.Id, i.TableNumber, i.Status, i.CreatedAt, i.AttendedAt
            )).ToList();

            return Ok(dto);
        }

        [HttpPatch("{callId:guid}/attend")]
        public async Task<IActionResult> Attend(Guid callId, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var hasAccess = await _db.WaiterCalls.AsNoTracking()
                .AnyAsync(c => c.Id == callId && c.RestaurantId == restaurantId, ct);
            if (!hasAccess)
            {
                return NotFound();
            }

            await _mediator.Send(new AttendWaiterCallCommand(callId), ct);
            return NoContent();
        }
    }
}
