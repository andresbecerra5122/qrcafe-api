using MediatR;

namespace QrCafe.Application.Ops.Commands.ToggleProductAvailability
{
    public record ToggleProductAvailabilityCommand(Guid ProductId, bool IsAvailable) : IRequest;
}
