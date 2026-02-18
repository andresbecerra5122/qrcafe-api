namespace QrCafe.Api.Dto.Menu
{
    public record MenuDto(
        Guid RestaurantId,
        string RestaurantName,
        string Currency,
        IReadOnlyList<CategoryDto> Categories,
        IReadOnlyList<ProductDto> Products
    );
}
