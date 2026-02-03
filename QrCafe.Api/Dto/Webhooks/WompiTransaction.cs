namespace QrCafe.Api.Dto.Webhooks
{
    public class WompiTransaction
    {
        public string Id { get; set; } = null!;
        public string Reference { get; set; } = null!;
        public string Status { get; set; } = null!; // APPROVED / DECLINED / ERROR / VOIDED
        public string Currency { get; set; } = null!;
        public long AmountInCents { get; set; }
    }
}
