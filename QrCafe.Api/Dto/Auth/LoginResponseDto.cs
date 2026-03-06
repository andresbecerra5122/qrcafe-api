namespace QrCafe.Api.Dto.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresInSeconds { get; set; }
        public AuthUserDto User { get; set; } = new();
    }

    public class AuthUserDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
