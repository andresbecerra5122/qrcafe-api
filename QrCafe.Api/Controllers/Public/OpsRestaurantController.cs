using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/restaurant")]
    [Authorize(Policy = AuthConstants.PolicyStaffAny)]
    public class OpsRestaurantController : ControllerBase
    {
        private readonly QrCafeDbContext _db;
        public OpsRestaurantController(QrCafeDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Guid restaurantId, CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var r = await _db.Restaurants.AsNoTracking()
                .Where(x => x.Id == restaurantId && x.IsActive)
                .Select(x => new { x.Id, x.Name, x.Currency })
                .SingleOrDefaultAsync(ct);

            if (r is null) return NotFound();
            return Ok(r);
        }
    }
}
