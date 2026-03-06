using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QrCafe.Domain.Entities;

namespace QrCafe.Api.Auth
{
    public interface IJwtTokenService
    {
        string CreateToken(StaffUser staff);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public string CreateToken(StaffUser staff)
        {
            var now = DateTime.UtcNow;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var role = staff.Role.ToString();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, staff.Id.ToString()),
                new Claim(AuthConstants.EmailClaim, staff.Email),
                new Claim(AuthConstants.RestaurantIdClaim, staff.RestaurantId.ToString()),
                new Claim(AuthConstants.RoleClaim, role),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_options.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
