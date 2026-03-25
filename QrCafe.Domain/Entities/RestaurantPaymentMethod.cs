namespace QrCafe.Domain.Entities
{
    public class RestaurantPaymentMethod
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Code { get; set; } = null!;
        public string Label { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public int Sort { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
