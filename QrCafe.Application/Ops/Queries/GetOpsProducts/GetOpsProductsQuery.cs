using MediatR;

namespace QrCafe.Application.Ops.Queries.GetOpsProducts
{
    public record GetOpsProductsQuery(Guid RestaurantId, bool IncludeInactive = false) : IRequest<GetOpsProductsResult>;

    public record GetOpsProductsResult(IReadOnlyList<OpsProductItem> Items);

    public record OpsProductItem(
        Guid Id,
        string Name,
        string? Description,
        string? CategoryName,
        decimal Price,
        bool IsAvailable,
        bool IsActive,
        string? ImageUrl,
        int Sort
    );
}
