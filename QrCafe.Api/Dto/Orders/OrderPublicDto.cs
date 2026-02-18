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
        decimal Total,
        DateTimeOffset CreatedAt,
        long OrderNumber
    );
}
