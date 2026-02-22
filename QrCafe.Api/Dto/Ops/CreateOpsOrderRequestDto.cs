namespace QrCafe.Api.Dto.Ops
{
    public record CreateOpsOrderRequestDto(
        Guid RestaurantId,
        int? TableNumber,
        string? CustomerName,
        string? Notes,
        IReadOnlyList<CreateOpsOrderItemDto> Items
    );

    public record CreateOpsOrderItemDto(
        Guid ProductId,
        int Qty,
        string? Notes
    );
}
