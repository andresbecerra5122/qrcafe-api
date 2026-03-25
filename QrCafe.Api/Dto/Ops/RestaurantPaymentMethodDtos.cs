namespace QrCafe.Api.Dto.Ops
{
    public record RestaurantPaymentMethodDto(Guid Id, string Code, string Label, int Sort);
    public record CreateRestaurantPaymentMethodRequestDto(string Label);
}
