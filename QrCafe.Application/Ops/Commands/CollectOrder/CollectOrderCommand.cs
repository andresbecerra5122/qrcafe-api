using MediatR;

namespace QrCafe.Application.Ops.Commands.CollectOrder
{
    public record CollectOrderCommand(Guid OrderId, string PaymentMethod) : IRequest;
}
