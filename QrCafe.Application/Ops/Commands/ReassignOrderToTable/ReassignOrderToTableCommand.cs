using MediatR;

namespace QrCafe.Application.Ops.Commands.ReassignOrderToTable
{
    public record ReassignOrderToTableCommand(Guid OrderId, Guid RestaurantId, int TargetTableNumber) : IRequest;
}
