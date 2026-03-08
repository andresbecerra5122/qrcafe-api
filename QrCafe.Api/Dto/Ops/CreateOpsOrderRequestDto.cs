namespace QrCafe.Api.Dto.Ops
{
    public record CreateOpsOrderRequestDto(
        Guid RestaurantId,
        string? OrderType,
        int? TableNumber,
        string? CustomerName,
        string? Notes,
        string? DeliveryAddress,
        string? DeliveryReference,
        string? DeliveryPhone,
        IReadOnlyList<CreateOpsOrderItemDto> Items
    );

    public record CreateOpsOrderItemDto(
        Guid ProductId,
        int Qty,
        string? Notes
    );
}
