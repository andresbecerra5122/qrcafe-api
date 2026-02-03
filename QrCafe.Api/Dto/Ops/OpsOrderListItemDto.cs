namespace QrCafe.Api.Dto.Ops
{
    public record OpsOrderListItemDto(
    Guid OrderId,
    string OrderType,
    int? TableNumber,
    string? CustomerName,
    string Status,
    string Currency,
    decimal Total,
    DateTimeOffset CreatedAt
);
}
