using MediatR;

namespace QrCafe.Application.Ops.Commands.SyncActiveTablesCount
{
    public record SyncActiveTablesCountCommand(Guid RestaurantId, int ActiveCount) : IRequest<int>;
}
