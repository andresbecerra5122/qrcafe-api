using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Application.Payments.Commands.CreateWompiCheckout;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("public/orders/{orderId:guid}/wompi")]
    public class PublicPaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicPaymentsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckout(Guid orderId, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateWompiCheckoutCommand(orderId), ct);
            return Ok(result);
        }
    }
}
