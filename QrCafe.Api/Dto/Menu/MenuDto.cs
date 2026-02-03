namespace QrCafe.Api.Dto.Menu
{
    public record MenuDto(
        Guid RestaurantId,
        IReadOnlyList<CategoryDto> Categories,
        IReadOnlyList<ProductDto> Products
    );
}
