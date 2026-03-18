namespace QrCafe.Api.Dto.Menu
{
    public record MenuDto(
        Guid RestaurantId,
        string RestaurantName,
        string Currency,
        bool EnableDineIn,
        bool EnableDelivery,
        bool EnableDeliveryCash,
        bool EnableDeliveryCard,
        bool EnablePayAtCashier,
        IReadOnlyList<CategoryDto> Categories,
        IReadOnlyList<ProductDto> Products
    );
}
