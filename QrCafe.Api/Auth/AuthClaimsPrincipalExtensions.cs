using System.Security.Claims;

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
    }
}
