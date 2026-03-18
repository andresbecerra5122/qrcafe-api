using MediatR;

namespace QrCafe.Application.Ops.Commands.SetDeliveryFee
{
    public record SetDeliveryFeeCommand(Guid OrderId, decimal DeliveryFee) : IRequest;
}
