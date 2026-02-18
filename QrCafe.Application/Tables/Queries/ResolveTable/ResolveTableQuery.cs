using MediatR;

namespace QrCafe.Application.Tables.Queries.ResolveTable
{
    public record ResolveTableQuery(Guid RestaurantId, string Number) : IRequest<ResolveTableResult?>;
}
