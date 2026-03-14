using MediatR;
using QrCafe.Application.Orders.Queries.GetOrderById;

namespace QrCafe.Application.Orders.Queries.GetActiveTableOrder
{
    public record GetActiveTableOrderQuery(Guid RestaurantId, string TableToken) : IRequest<GetOrderByIdResult?>;
}
