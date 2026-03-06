namespace QrCafe.Api.Dto.Orders
{
    public record OrderPublicDto(
        Guid OrderId,
        string OrderType,
        int? TableNumber,
        string? CustomerName,
        string Status,
        string? PaymentStatus,
        string? PaymentMethod,
        string Currency,
        decimal Subtotal,
        decimal Tax,
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
        decimal LineTotal
    );
}
