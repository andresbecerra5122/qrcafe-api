using MediatR;

namespace QrCafe.Application.Ops.Commands.UpdateOrderItemDelivered
{
    public record UpdateOrderItemDeliveredCommand(Guid OrderId, Guid OrderItemId, bool IsDelivered) : IRequest;
}
