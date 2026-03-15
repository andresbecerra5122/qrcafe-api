namespace QrCafe.Api.Dto.Ops
{
    public record OpsOrderItemDto(Guid ItemId, string ProductName, int Qty, string? Notes, string PrepStation, bool IsPrepared, bool IsDelivered, bool IsDone);

    public record OpsOrderListItemDto(
        Guid OrderId,
        string OrderType,
        int? TableNumber,
        string? CustomerName,
        string? DeliveryAddress,
        string? DeliveryReference,
        string? DeliveryPhone,
        string Status,
        string? PaymentMethod,
        DateTimeOffset? PaymentRequestedAt,
        string Currency,
        decimal Total,
        DateTimeOffset CreatedAt,
        IReadOnlyList<OpsOrderItemDto> Items
    );
}
