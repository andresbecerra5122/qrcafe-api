namespace QrCafe.Application.Orders.Queries.GetOrderById
{
    public record GetOrderByIdResult(
        Guid OrderId,
        string OrderType,
        int? TableNumber,
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
        IReadOnlyList<OrderItemResult> Items
    );

    public record OrderItemResult(
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
