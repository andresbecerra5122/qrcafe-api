namespace QrCafe.Api.Dto.Orders
{
    public record CreateOrderRequestDto(
        Guid RestaurantId,
        string OrderType,           // "DINE_IN" | "TAKEAWAY"
        string? TableToken,         // requerido si DINE_IN
        string? CustomerName,       // opcional
        string? Notes,              // opcional
        IReadOnlyList<CreateOrderItemRequestDto> Items
    );
}
