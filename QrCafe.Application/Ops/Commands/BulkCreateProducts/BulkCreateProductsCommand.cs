using MediatR;

namespace QrCafe.Application.Ops.Commands.BulkCreateProducts
{
    public record BulkCreateProductsCommand(
        Guid RestaurantId,
        IReadOnlyList<BulkCreateProductInput> Products
    ) : IRequest<BulkCreateProductsResult>;

    public record BulkCreateProductInput(
        string Name,
        string? Description,
        string? CategoryName,
        string? PrepStation,
        decimal Price,
        string? ImageUrl,
        int Sort,
        bool IsAvailable
    );

    public record BulkCreateProductsResult(int CreatedCount);
}
