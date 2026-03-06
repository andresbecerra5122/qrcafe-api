using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/tables")]
    [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
    public class OpsTablesController : ControllerBase
    {
        private readonly QrCafeDbContext _db;
        public OpsTablesController(QrCafeDbContext db) => _db = db;

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
    }

    public record OpsTableItem(Guid Id, int Number, string Token);
}
