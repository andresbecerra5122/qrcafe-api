using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
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
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Currency,
                    x.EnableDineIn,
                    x.EnableDelivery,
                    x.EnableDeliveryCash,
                    x.EnableDeliveryCard,
                    x.EnableKitchenBarSplit
                })
                .SingleOrDefaultAsync(ct);

            if (r is null) return NotFound();
            return Ok(r);
        }

        [HttpPatch("settings")]
        [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
        public async Task<IActionResult> UpdateSettings([FromQuery] Guid restaurantId, [FromBody] UpdateRestaurantSettingsRequestDto req, CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var restaurant = await _db.Restaurants.SingleOrDefaultAsync(r => r.Id == restaurantId && r.IsActive, ct);
            if (restaurant is null)
            {
                return NotFound();
            }

            if (req.EnableDineIn.HasValue)
            {
                restaurant.EnableDineIn = req.EnableDineIn.Value;
            }

            if (req.EnableDelivery.HasValue)
            {
                restaurant.EnableDelivery = req.EnableDelivery.Value;
            }
            if (req.EnableDeliveryCash.HasValue)
            {
                restaurant.EnableDeliveryCash = req.EnableDeliveryCash.Value;
            }
            if (req.EnableDeliveryCard.HasValue)
            {
                restaurant.EnableDeliveryCard = req.EnableDeliveryCard.Value;
            }
            if (req.EnableKitchenBarSplit.HasValue)
            {
                restaurant.EnableKitchenBarSplit = req.EnableKitchenBarSplit.Value;
            }

            restaurant.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Ok(new
            {
                restaurant.Id,
                restaurant.Name,
                restaurant.Currency,
                restaurant.EnableDineIn,
                restaurant.EnableDelivery,
                restaurant.EnableDeliveryCash,
                restaurant.EnableDeliveryCard,
                restaurant.EnableKitchenBarSplit
            });
        }
    }
}
