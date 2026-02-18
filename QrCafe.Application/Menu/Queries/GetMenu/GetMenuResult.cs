using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Menu.Queries.GetMenu
{
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
        IReadOnlyList<GetMenuCategoryItem> Categories,
        IReadOnlyList<GetMenuProductItem> Products
    );
}
