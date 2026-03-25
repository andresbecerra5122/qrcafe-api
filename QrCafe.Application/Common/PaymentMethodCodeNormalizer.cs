using System.Text;

namespace QrCafe.Application.Common
{
    public static class PaymentMethodCodeNormalizer
    {
        public static string ToCode(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentException("Payment method label is required.");

            var sb = new StringBuilder(raw.Length);
            foreach (var ch in raw.Trim().ToUpperInvariant())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                    continue;
                }

                if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_')
                {
                    if (sb.Length > 0 && sb[^1] != '_')
                        sb.Append('_');
                }
            }

            var code = sb.ToString().Trim('_');
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Payment method label is invalid.");

            return code.Length <= 30 ? code : code[..30].TrimEnd('_');
        }
    }
}
