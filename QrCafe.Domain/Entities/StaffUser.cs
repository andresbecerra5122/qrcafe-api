using QrCafe.Domain.Entities.Enums;

namespace QrCafe.Domain.Entities
{
    public class StaffUser
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public StaffRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
    }
}
