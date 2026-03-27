using MediatR;

namespace QrCafe.Application.Ops.Commands.CollectOrder
{
    public record CollectOrderCommand(Guid OrderId, string PaymentMethod, string? TipMode, decimal? TipAmount) : IRequest;
}
