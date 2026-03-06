using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Api.Mappers;
using QrCafe.Application.Ops.Commands.UpdateOrderStatus;
using QrCafe.Application.Ops.Commands.CollectOrder;
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
            CancellationToken ct)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var result = await _mediator.Send(new GetOpsOrdersQuery(restaurantId, status), ct);
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

            string? tableToken = null;
            string orderType = "TAKEAWAY";

            if (req.TableNumber.HasValue)
            {
                var table = await _db.Tables.AsNoTracking()
                    .SingleOrDefaultAsync(t => t.RestaurantId == req.RestaurantId
                        && t.Number == req.TableNumber.Value && t.IsActive, ct);

                if (table is null) return BadRequest("Mesa no encontrada.");

                tableToken = table.Token;
                orderType = "DINE_IN";
            }

            var input = new CreateOrderInput(
                req.RestaurantId,
                orderType,
                tableToken,
                req.CustomerName,
                req.Notes,
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

            await _mediator.Send(new CollectOrderCommand(orderId, req.PaymentMethod), ct);
            return NoContent();
        }
    }

    public record CollectOrderDto(string PaymentMethod);
}
