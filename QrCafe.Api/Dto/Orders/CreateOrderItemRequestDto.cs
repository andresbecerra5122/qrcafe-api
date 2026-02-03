namespace QrCafe.Api.Dto.Orders
{
    public record CreateOrderItemRequestDto(
        Guid ProductId,
        int Qty,
        string? Notes
    );
}
