using MediatR;

namespace QrCafe.Application.WaiterCalls.Queries.GetWaiterCalls
{
    public record GetWaiterCallsQuery(Guid RestaurantId, string? Status) : IRequest<GetWaiterCallsResult>;

    public record GetWaiterCallsResult(IReadOnlyList<WaiterCallItem> Items);

    public record WaiterCallItem(
        Guid Id,
        Guid RestaurantId,
        int? TableNumber,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? AttendedAt
    );
}
