using QrCafe.Api.Dto.Menu;
using QrCafe.Application.Menu.Queries.GetMenu;

namespace QrCafe.Api.Mappers
{
    public static class MenuMapper
    {
        public static MenuDto ToDto(GetMenuResult r) => new(
            r.RestaurantId,
            r.RestaurantName,
            r.Currency,
            r.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.Sort)).ToList(),
            r.Products.Select(p => new ProductDto(
                p.Id, p.CategoryId, p.Name, p.Description, p.Price, p.IsAvailable, p.Sort, p.ImageUrl
            )).ToList()
        );
    }
}
