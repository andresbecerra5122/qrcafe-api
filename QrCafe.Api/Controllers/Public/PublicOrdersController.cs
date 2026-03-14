using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Dto.Orders;
using QrCafe.Api.Mappers;
using QrCafe.Application.Orders.Commands.CreateOrder;
using QrCafe.Application.Orders.Commands.RequestPayment;
using QrCafe.Application.Orders.Queries.GetActiveTableOrder;
using QrCafe.Application.Orders.Queries.GetOrderById;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("public/orders")]
    public class PublicOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicOrdersController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<ActionResult<CreateOrderResponseDto>> Create([FromBody] CreateOrderRequestDto req, CancellationToken ct)
        {
            var input = new CreateOrderInput(
                req.RestaurantId,
                req.OrderType,
                req.TableToken,
                req.CustomerName,
                req.Notes,
                req.DeliveryAddress,
                req.DeliveryReference,
                req.DeliveryPhone,
                req.PaymentMethod,
                req.Items.Select(i => new CreateOrderItemInput(i.ProductId, i.Qty, i.Notes)).ToList()
            );

            var result = await _mediator.Send(new CreateOrderCommand(input), ct);
            var dto = OrdersMapper.ToDto(result);

            return CreatedAtAction(nameof(GetById), new { orderId = dto.OrderId }, dto);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<OrderPublicDto>> GetById(Guid orderId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOrderByIdQuery(orderId), ct);
            if (result is null) return NotFound();

            return Ok(OrdersMapper.ToDto(result));
        }

        [HttpGet("active-by-table")]
        public async Task<ActionResult<OrderPublicDto>> GetActiveByTable(
            [FromQuery] Guid restaurantId,
            [FromQuery] string tableToken,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActiveTableOrderQuery(restaurantId, tableToken), ct);
            if (result is null) return NotFound();

            return Ok(OrdersMapper.ToDto(result));
        }

        [HttpPost("{orderId:guid}/request-payment")]
        public async Task<IActionResult> RequestPayment(Guid orderId, [FromBody] RequestPaymentDto req, CancellationToken ct)
        {
            await _mediator.Send(new RequestPaymentCommand(orderId, req.PaymentMethod), ct);
            return NoContent();
        }
    }
}
