using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Menu.Queries.GetMenu
{
    public record GetMenuPaymentMethodItem(Guid Id, string Code, string Label, int Sort);

    public record GetMenuCategoryItem(Guid Id, string Name, int Sort);

    public record GetMenuProductItem(
        Guid Id,
        Guid? CategoryId,
        string Name,
        string? Description,
        decimal Price,
        bool IsAvailable,
        int Sort,
        string? ImageUrl
    );

    public record GetMenuResult(
        Guid RestaurantId,
        string RestaurantName,
        string Currency,
        bool EnableDineIn,
        bool EnableDelivery,
        bool EnableDeliveryCash,
        bool EnableDeliveryCard,
        bool EnablePayAtCashier,
        int AvgPreparationMinutes,
        decimal SuggestedTipPercent,
        IReadOnlyList<GetMenuPaymentMethodItem> PaymentMethods,
        IReadOnlyList<GetMenuCategoryItem> Categories,
        IReadOnlyList<GetMenuProductItem> Products
    );
}
