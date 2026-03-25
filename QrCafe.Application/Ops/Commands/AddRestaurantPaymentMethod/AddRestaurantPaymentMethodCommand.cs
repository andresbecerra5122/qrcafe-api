using MediatR;
using QrCafe.Application.Ops.Queries.GetRestaurantPaymentMethods;

namespace QrCafe.Application.Ops.Commands.AddRestaurantPaymentMethod
{
    public record AddRestaurantPaymentMethodCommand(Guid RestaurantId, string Label) : IRequest<RestaurantPaymentMethodItem>;
}
