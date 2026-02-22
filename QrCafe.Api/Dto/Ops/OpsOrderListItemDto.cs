namespace QrCafe.Api.Dto.Ops
{
    public record OpsOrderItemDto(string ProductName, int Qty, string? Notes);

    public record OpsOrderListItemDto(
        Guid OrderId,
        string OrderType,
        int? TableNumber,
        string? CustomerName,
        string Status,
        string? PaymentMethod,
        DateTimeOffset? PaymentRequestedAt,
        string Currency,
        decimal Total,
        DateTimeOffset CreatedAt,
        IReadOnlyList<OpsOrderItemDto> Items
    );
}
