using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Api.Mappers;
using QrCafe.Application.Ops.Commands.UpdateOrderStatus;
using QrCafe.Application.Ops.Commands.UpdateOrderItemPrepared;
using QrCafe.Application.Ops.Commands.UpdateOrderItemDelivered;
using QrCafe.Application.Ops.Commands.CollectOrder;
using QrCafe.Application.Ops.Commands.SetDeliveryFee;
using QrCafe.Application.Ops.Commands.ReassignOrderToTable;
using QrCafe.Application.Ops.Queries.GetOpsOrders;
using QrCafe.Application.Orders.Commands.CreateOrder;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/orders")]
    [Authorize(Policy = AuthConstants.PolicyStaffAny)]
    public class OpsOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly QrCafeDbContext _db;
        public OpsOrdersController(IMediator mediator, QrCafeDbContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OpsOrderListItemDto>>> Get(
            [FromQuery] Guid restaurantId,
            [FromQuery] string? status,
            [FromQuery] string? orderType,
            CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var result = await _mediator.Send(new GetOpsOrdersQuery(restaurantId, status, orderType), ct);
            var dto = result.Items.Select(OpsOrdersMapper.ToDto).ToList();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOpsOrderRequestDto req, CancellationToken ct)
        {
            if (User.GetRestaurantId() != req.RestaurantId)
            {
                return Forbid();
            }

            var restaurant = await _db.Restaurants.AsNoTracking()
                .SingleOrDefaultAsync(r => r.Id == req.RestaurantId && r.IsActive, ct);
            if (restaurant is null)
            {
                return NotFound("Restaurante no encontrado.");
            }

            var requestedType = req.OrderType?.Trim();
            if (string.IsNullOrWhiteSpace(requestedType))
            {
                if (req.TableNumber.HasValue)
                {
                    requestedType = "DINE_IN";
                }
                else if (restaurant.EnableDelivery)
                {
                    requestedType = "DELIVERY";
                }
                else
                {
                    requestedType = "TAKEAWAY";
                }
            }

            if (!Enum.TryParse<QrCafe.Domain.Entities.Enums.OrderType>(requestedType, true, out var parsedOrderType))
            {
                return BadRequest("Tipo de orden inválido.");
            }

            string? tableToken = null;
            if (parsedOrderType == QrCafe.Domain.Entities.Enums.OrderType.DINE_IN)
            {
                if (!restaurant.EnableDineIn)
                {
                    return BadRequest("DINE_IN está deshabilitado para este restaurante.");
                }

                if (!req.TableNumber.HasValue)
                {
                    return BadRequest("tableNumber es requerido para DINE_IN.");
                }

                var table = await _db.Tables.AsNoTracking()
                    .SingleOrDefaultAsync(t => t.RestaurantId == req.RestaurantId
                        && t.Number == req.TableNumber.Value && t.IsActive, ct);

                if (table is null) return BadRequest("Mesa no encontrada.");

                tableToken = table.Token;
            }

            if (parsedOrderType == QrCafe.Domain.Entities.Enums.OrderType.DELIVERY && !restaurant.EnableDelivery)
            {
                return BadRequest("DELIVERY está deshabilitado para este restaurante.");
            }

            var input = new CreateOrderInput(
                req.RestaurantId,
                parsedOrderType.ToString(),
                tableToken,
                req.CustomerName,
                req.Notes,
                req.DeliveryAddress,
                req.DeliveryReference,
                req.DeliveryPhone,
                null,
                req.Items.Select(i => new CreateOrderItemInput(i.ProductId, i.Qty, i.Notes)).ToList()
            );

            var result = await _mediator.Send(new CreateOrderCommand(input), ct);

            return Ok(new { result.OrderId, result.Status, result.OrderNumber, result.Total });
        }

        [HttpPatch("{orderId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequestDto req, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var hasAccess = await _db.Orders.AsNoTracking()
                .AnyAsync(o => o.Id == orderId && o.RestaurantId == restaurantId, ct);
            if (!hasAccess)
            {
                return NotFound();
            }

            await _mediator.Send(new UpdateOrderStatusCommand(orderId, req.Status), ct);
            return NoContent();
        }

        [HttpPatch("{orderId:guid}/items/{itemId:guid}/prepared")]
        [Authorize(Policy = AuthConstants.PolicyKitchenOrAdmin)]
        public async Task<IActionResult> UpdateItemPrepared(Guid orderId, Guid itemId, [FromBody] UpdateOrderItemStateDto req, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var hasAccess = await _db.Orders.AsNoTracking()
                .AnyAsync(o => o.Id == orderId && o.RestaurantId == restaurantId, ct);
            if (!hasAccess) return NotFound();

            await _mediator.Send(new UpdateOrderItemPreparedCommand(orderId, itemId, req.Value), ct);
            return NoContent();
        }

        [HttpPatch("{orderId:guid}/items/{itemId:guid}/delivered")]
        [Authorize(Policy = AuthConstants.PolicyWaiterOrAdmin)]
        public async Task<IActionResult> UpdateItemDelivered(Guid orderId, Guid itemId, [FromBody] UpdateOrderItemStateDto req, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var hasAccess = await _db.Orders.AsNoTracking()
                .AnyAsync(o => o.Id == orderId && o.RestaurantId == restaurantId, ct);
            if (!hasAccess) return NotFound();

            await _mediator.Send(new UpdateOrderItemDeliveredCommand(orderId, itemId, req.Value), ct);
            return NoContent();
        }

        [HttpPatch("{orderId:guid}/collect")]
        [Authorize(Policy = AuthConstants.PolicyWaiterOrAdmin)]
        public async Task<IActionResult> Collect(Guid orderId, [FromBody] CollectOrderDto req, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var hasAccess = await _db.Orders.AsNoTracking()
                .AnyAsync(o => o.Id == orderId && o.RestaurantId == restaurantId, ct);
            if (!hasAccess)
            {
                return NotFound();
            }

            await _mediator.Send(new CollectOrderCommand(orderId, req.PaymentMethod, req.TipMode, req.TipAmount), ct);
            return NoContent();
        }

        [HttpPatch("{orderId:guid}/delivery-fee")]
        [Authorize(Policy = AuthConstants.PolicyDeliveryOrAdmin)]
        public async Task<IActionResult> SetDeliveryFee(Guid orderId, [FromBody] UpdateDeliveryFeeRequestDto req, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var hasAccess = await _db.Orders.AsNoTracking()
                .AnyAsync(o => o.Id == orderId && o.RestaurantId == restaurantId, ct);
            if (!hasAccess)
            {
                return NotFound();
            }

            await _mediator.Send(new SetDeliveryFeeCommand(orderId, req.DeliveryFee), ct);
            return NoContent();
        }

        [HttpPatch("{orderId:guid}/table")]
        [Authorize(Policy = AuthConstants.PolicyWaiterOrAdmin)]
        public async Task<IActionResult> ReassignTable(
            Guid orderId,
            [FromBody] ReassignOrderTableRequestDto req,
            CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var hasAccess = await _db.Orders.AsNoTracking()
                .AnyAsync(o => o.Id == orderId && o.RestaurantId == restaurantId, ct);
            if (!hasAccess)
            {
                return NotFound();
            }

            try
            {
                await _mediator.Send(new ReassignOrderToTableCommand(orderId, restaurantId, req.TableNumber), ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }
    }

    public record ReassignOrderTableRequestDto(int TableNumber);
    public record CollectOrderDto(string PaymentMethod, string? TipMode = null, decimal? TipAmount = null);
    public record UpdateOrderItemStateDto(bool Value);
}
