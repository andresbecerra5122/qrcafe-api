using System.Security.Cryptography;
using System.Text;

namespace QrCafe.Infrastructure.Payments.Wompi
{
    public static class WompiIntegrity
    {
        public static string BuildSignature(string reference, long amountInCents, string currency, string integritySecret)
        {
            var raw = $"{reference}{amountInCents}{currency}{integritySecret}";

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
