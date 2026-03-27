namespace QrCafe.Api.Dto.Orders
{
    public record RequestPaymentDto(string PaymentMethod, string? TipMode = null, decimal? TipAmount = null);
}
