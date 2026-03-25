using MediatR;

namespace QrCafe.Application.Ops.Commands.DeleteRestaurantPaymentMethod
{
    public record DeleteRestaurantPaymentMethodCommand(Guid RestaurantId, Guid MethodId) : IRequest;
}
