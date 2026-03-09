using MediatR;

namespace QrCafe.Application.Ops.Commands.CreateRestaurantOnboarding
{
    public record CreateRestaurantOnboardingCommand(CreateRestaurantOnboardingInput Input)
        : IRequest<CreateRestaurantOnboardingResult>;
}
