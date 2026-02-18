namespace QrCafe.Api.Dto.Ops
{
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
    DateTimeOffset CreatedAt
);
}
