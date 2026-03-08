namespace QrCafe.Api.Dto.Orders
{
    public record CreateOrderRequestDto(
        Guid RestaurantId,
        string OrderType,           // "DINE_IN" | "TAKEAWAY" | "DELIVERY"
        string? TableToken,         // requerido si DINE_IN
        string? CustomerName,       // opcional
        string? Notes,              // opcional
        string? DeliveryAddress,    // requerido si DELIVERY
        string? DeliveryReference,  // opcional
        string? DeliveryPhone,      // requerido si DELIVERY
        string? PaymentMethod,      // requerido si DELIVERY
        IReadOnlyList<CreateOrderItemRequestDto> Items
    );
}
