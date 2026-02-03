namespace QrCafe.Api.Dto.Restaurants
{
    public record RestaurantPublicDto(
        Guid Id,
        string Name,
        string Slug,
        string CountryCode,
        string Currency,
        string TimeZone,
        decimal TaxRate
    );
}
