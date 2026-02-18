namespace QrCafe.Domain.Entities
{
    public class WaiterCall
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid? TableId { get; set; }
        public int? TableNumber { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING | ATTENDED
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? AttendedAt { get; set; }
    }
}
