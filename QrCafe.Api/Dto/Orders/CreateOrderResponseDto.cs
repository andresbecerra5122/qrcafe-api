namespace QrCafe.Api.Dto.Orders
{
    public record CreateOrderResponseDto(
        Guid OrderId,
        string Status,
        string Currency,
        decimal Subtotal,
        decimal Tax,
        decimal Total
    );
}
