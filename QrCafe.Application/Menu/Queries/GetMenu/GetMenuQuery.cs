using MediatR;

namespace QrCafe.Application.Menu.Queries.GetMenu
{
    public record GetMenuQuery(Guid RestaurantId) : IRequest<GetMenuResult?>;
}
