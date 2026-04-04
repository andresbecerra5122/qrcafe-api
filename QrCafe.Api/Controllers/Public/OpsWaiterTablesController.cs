using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Public
{
    /// <summary>
    /// Listado de mesas para meseros (reasignación de cuenta). Admin usa ops/tables.
    /// </summary>
    [ApiController]
    [Route("ops/waiter-tables")]
    [Authorize(Policy = AuthConstants.PolicyWaiterOrAdmin)]
    public class OpsWaiterTablesController : ControllerBase
    {
        private readonly QrCafeDbContext _db;

        public OpsWaiterTablesController(QrCafeDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<WaiterTableItemDto>>> Get(
            [FromQuery] Guid restaurantId,
            CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var tables = await _db.Tables
                .AsNoTracking()
                .Where(t => t.RestaurantId == restaurantId && t.IsActive)
                .OrderBy(t => t.Number)
                .Select(t => new WaiterTableItemDto(t.Number, t.Token))
                .ToListAsync(ct);

            return Ok(tables);
        }
    }

    public record WaiterTableItemDto(int Number, string Token);
}
