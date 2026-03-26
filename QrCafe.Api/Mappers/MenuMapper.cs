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
            r.EnableDineIn,
            r.EnableDelivery,
            r.EnableDeliveryCash,
            r.EnableDeliveryCard,
            r.EnablePayAtCashier,
            r.AvgPreparationMinutes,
            r.PaymentMethods.Select(pm => new MenuPaymentMethodDto(pm.Id, pm.Code, pm.Label, pm.Sort)).ToList(),
            r.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.Sort)).ToList(),
            r.Products.Select(p => new ProductDto(
                p.Id, p.CategoryId, p.Name, p.Description, p.Price, p.IsAvailable, p.Sort, p.ImageUrl
            )).ToList()
        );
    }
}
