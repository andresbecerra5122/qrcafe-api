using MediatR;

namespace QrCafe.Application.Ops.Commands.UpdateOrderItemPrepared
{
    public record UpdateOrderItemPreparedCommand(Guid OrderId, Guid OrderItemId, bool IsPrepared) : IRequest;
}
