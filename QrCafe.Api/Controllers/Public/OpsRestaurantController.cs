using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Application.Ops.Commands.AddRestaurantPaymentMethod;
using QrCafe.Application.Ops.Commands.DeleteRestaurantPaymentMethod;
using QrCafe.Application.Ops.Queries.GetRestaurantPaymentMethods;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/restaurant")]
    [Authorize(Policy = AuthConstants.PolicyStaffAny)]
    public class OpsRestaurantController : ControllerBase
    {
        private readonly QrCafeDbContext _db;
        private readonly IMediator _mediator;
        public OpsRestaurantController(QrCafeDbContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

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
                    x.EnablePayAtCashier,
                    x.EnableKitchenBarSplit,
                    x.AvgPreparationMinutes,
                    x.SuggestedTipPercent
                })
                .SingleOrDefaultAsync(ct);

            if (r is null) return NotFound();
            var methods = await _mediator.Send(new GetRestaurantPaymentMethodsQuery(restaurantId), ct);
            return Ok(new
            {
                r.Id,
                r.Name,
                r.Currency,
                r.EnableDineIn,
                r.EnableDelivery,
                r.EnableDeliveryCash,
                r.EnableDeliveryCard,
                r.EnablePayAtCashier,
                r.EnableKitchenBarSplit,
                r.AvgPreparationMinutes,
                r.SuggestedTipPercent,
                PaymentMethods = methods.Select(m => new RestaurantPaymentMethodDto(m.Id, m.Code, m.Label, m.Sort)).ToList()
            });
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
            if (req.EnablePayAtCashier.HasValue)
            {
                restaurant.EnablePayAtCashier = req.EnablePayAtCashier.Value;
            }
            if (req.EnableKitchenBarSplit.HasValue)
            {
                restaurant.EnableKitchenBarSplit = req.EnableKitchenBarSplit.Value;
            }
            if (req.AvgPreparationMinutes.HasValue)
            {
                var minutes = req.AvgPreparationMinutes.Value;
                if (minutes < 1 || minutes > 600)
                {
                    return BadRequest(new { error = "AvgPreparationMinutes must be between 1 and 600." });
                }
                restaurant.AvgPreparationMinutes = minutes;
            }
            if (req.SuggestedTipPercent.HasValue)
            {
                var percent = req.SuggestedTipPercent.Value;
                if (percent < 0m || percent > 100m)
                {
                    return BadRequest(new { error = "SuggestedTipPercent must be between 0 and 100." });
                }
                restaurant.SuggestedTipPercent = decimal.Round(percent, 2);
            }

            restaurant.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            var methods = await _mediator.Send(new GetRestaurantPaymentMethodsQuery(restaurantId), ct);
            return Ok(new
            {
                restaurant.Id,
                restaurant.Name,
                restaurant.Currency,
                restaurant.EnableDineIn,
                restaurant.EnableDelivery,
                restaurant.EnableDeliveryCash,
                restaurant.EnableDeliveryCard,
                restaurant.EnablePayAtCashier,
                restaurant.EnableKitchenBarSplit,
                restaurant.AvgPreparationMinutes,
                restaurant.SuggestedTipPercent,
                PaymentMethods = methods.Select(m => new RestaurantPaymentMethodDto(m.Id, m.Code, m.Label, m.Sort)).ToList()
            });
        }

        [HttpGet("payment-methods")]
        public async Task<ActionResult<IReadOnlyList<RestaurantPaymentMethodDto>>> GetPaymentMethods([FromQuery] Guid restaurantId, CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var methods = await _mediator.Send(new GetRestaurantPaymentMethodsQuery(restaurantId), ct);
            return Ok(methods.Select(m => new RestaurantPaymentMethodDto(m.Id, m.Code, m.Label, m.Sort)).ToList());
        }

        [HttpPost("payment-methods")]
        [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
        public async Task<ActionResult<RestaurantPaymentMethodDto>> AddPaymentMethod(
            [FromQuery] Guid restaurantId,
            [FromBody] CreateRestaurantPaymentMethodRequestDto req,
            CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var item = await _mediator.Send(new AddRestaurantPaymentMethodCommand(restaurantId, req.Label), ct);
            return Ok(new RestaurantPaymentMethodDto(item.Id, item.Code, item.Label, item.Sort));
        }

        [HttpDelete("payment-methods/{methodId:guid}")]
        [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
        public async Task<IActionResult> DeletePaymentMethod([FromQuery] Guid restaurantId, Guid methodId, CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            await _mediator.Send(new DeleteRestaurantPaymentMethodCommand(restaurantId, methodId), ct);
            return NoContent();
        }
    }
}
