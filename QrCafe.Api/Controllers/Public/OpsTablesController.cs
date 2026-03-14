using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Application.Ops.Commands.SyncActiveTablesCount;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/tables")]
    [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
    public class OpsTablesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly QrCafeDbContext _db;
        public OpsTablesController(IMediator mediator, QrCafeDbContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OpsTableItem>>> Get(
            [FromQuery] Guid restaurantId,
            CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var tables = await _db.Tables
                .Where(t => t.RestaurantId == restaurantId && t.IsActive)
                .OrderBy(t => t.Number)
                .Select(t => new OpsTableItem(t.Id, t.Number, t.Token))
                .ToListAsync(ct);

            return Ok(tables);
        }

        [HttpPatch("active-count")]
        public async Task<ActionResult<UpdateActiveTablesCountResponseDto>> UpdateActiveCount(
            [FromQuery] Guid restaurantId,
            [FromBody] UpdateActiveTablesCountRequestDto req,
            CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var activeCount = await _mediator.Send(
                new SyncActiveTablesCountCommand(restaurantId, req.ActiveCount),
                ct);

            return Ok(new UpdateActiveTablesCountResponseDto(activeCount));
        }
    }

    public record OpsTableItem(Guid Id, int Number, string Token);
}
