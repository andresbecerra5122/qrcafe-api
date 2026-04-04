namespace QrCafe.Api.Dto.Orders
{
    public record OrderPublicDto(
        Guid OrderId,
        string OrderType,
        int? TableNumber,
        string? TableToken,
        string? CustomerName,
        string? DeliveryAddress,
        string? DeliveryReference,
        string? DeliveryPhone,
        string Status,
        string? PaymentStatus,
        string? PaymentMethod,
        string Currency,
        decimal Subtotal,
        decimal Tax,
        decimal DeliveryFee,
        decimal TipAmount,
        string? TipSource,
        decimal Total,
        DateTimeOffset CreatedAt,
        long OrderNumber,
        string RestaurantName,
        IReadOnlyList<OrderItemPublicDto> Items
    );

    public record OrderItemPublicDto(
        string ProductName,
        int Qty,
        decimal UnitPrice,
        decimal LineTotal,
        string PrepStation,
        bool IsPrepared,
        bool IsDelivered,
        bool IsDone
    );
}
