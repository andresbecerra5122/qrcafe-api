using MediatR;

namespace QrCafe.Application.WaiterCalls.Commands.AttendWaiterCall
{
    public record AttendWaiterCallCommand(Guid WaiterCallId) : IRequest;
}
