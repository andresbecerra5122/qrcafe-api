using MediatR;

namespace QrCafe.Application.Ops.Queries.GetRestaurantPaymentMethods
{
    public record RestaurantPaymentMethodItem(Guid Id, string Code, string Label, int Sort);

    public record GetRestaurantPaymentMethodsQuery(Guid RestaurantId) : IRequest<IReadOnlyList<RestaurantPaymentMethodItem>>;
}
