using MediatR;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Dto.WaiterCalls;
using QrCafe.Application.WaiterCalls.Commands.CreateWaiterCall;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("public/waiter-calls")]
    public class PublicWaiterCallsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicWaiterCallsController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<ActionResult<CreateWaiterCallResponseDto>> Create(
            [FromBody] CreateWaiterCallRequestDto req,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new CreateWaiterCallCommand(req.RestaurantId, req.TableToken), ct);

            return Created("", new CreateWaiterCallResponseDto(result.WaiterCallId));
        }
    }
}
