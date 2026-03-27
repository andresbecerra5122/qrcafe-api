using MediatR;

namespace QrCafe.Application.Orders.Commands.RequestPayment
{
    public record RequestPaymentCommand(Guid OrderId, string PaymentMethod, string? TipMode, decimal? TipAmount) : IRequest;
}
