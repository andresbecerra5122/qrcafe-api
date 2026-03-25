namespace QrCafe.Api.Dto.Menu
{
    public record MenuPaymentMethodDto(Guid Id, string Code, string Label, int Sort);

    public record MenuDto(
        Guid RestaurantId,
        string RestaurantName,
        string Currency,
        bool EnableDineIn,
        bool EnableDelivery,
        bool EnableDeliveryCash,
        bool EnableDeliveryCard,
        bool EnablePayAtCashier,
        IReadOnlyList<MenuPaymentMethodDto> PaymentMethods,
        IReadOnlyList<CategoryDto> Categories,
        IReadOnlyList<ProductDto> Products
    );
}
