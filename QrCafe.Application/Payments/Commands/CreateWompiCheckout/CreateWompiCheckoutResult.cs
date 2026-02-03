

namespace QrCafe.Application.Payments.Commands.CreateWompiCheckout
{
    public record CreateWompiCheckoutResult(
         string PublicKey,
         string Reference,
         long AmountInCents,
         string Currency,
         string SignatureIntegrity,
         string RedirectUrl
     );
}
