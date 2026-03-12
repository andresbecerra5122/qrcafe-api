using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace QrCafe.Api.Auth
{
    public static class AuthClaimsPrincipalExtensions
    {
        public static Guid GetRestaurantId(this ClaimsPrincipal user)
        {
            var claimValue = user.FindFirstValue(AuthConstants.RestaurantIdClaim);
            return Guid.TryParse(claimValue, out var restaurantId) ? restaurantId : Guid.Empty;
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(AuthConstants.RoleClaim) ?? user.FindFirstValue(ClaimTypes.Role);
            return role ?? string.Empty;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(AuthConstants.EmailClaim) ?? user.FindFirstValue(ClaimTypes.Email);
            return email ?? string.Empty;
        }

        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var claimValue =
                user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claimValue, out var userId) ? userId : Guid.Empty;
        }
    }
}
