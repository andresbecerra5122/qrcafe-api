namespace QrCafe.Api.Dto.Ops
{
    public class StaffUserDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
    }
}
