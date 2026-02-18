using MediatR;

namespace QrCafe.Application.WaiterCalls.Commands.CreateWaiterCall
{
    public record CreateWaiterCallCommand(Guid RestaurantId, string? TableToken) : IRequest<CreateWaiterCallResult>;

    public record CreateWaiterCallResult(Guid WaiterCallId);
}
