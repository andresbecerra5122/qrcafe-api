

namespace QrCafe.Application.Restaurants.Queries.GetRestaurantBySlug
{
    public record GetRestaurantBySlugResult(
     Guid Id,
     string Name,
     string Slug,
     string CountryCode,
     string Currency,
     string TimeZone,
     decimal TaxRate
 );
}
