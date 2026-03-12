using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Auth;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Ops
{
    [ApiController]
    [Route("ops/auth")]
    public class OpsAuthController : ControllerBase
    {
        private readonly QrCafeDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtOptions _jwtOptions;

        public OpsAuthController(
            QrCafeDbContext db,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IOptions<JwtOptions> jwtOptions)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new { error = "Email and password are required." });
            }

            var staff = await _db.StaffUsers.SingleOrDefaultAsync(
                u => u.Email == email && u.IsActive,
                ct
            );

            if (staff is null || !_passwordHasher.Verify(req.Password, staff.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid credentials." });
            }

            var restaurantIsActive = await _db.Restaurants.AsNoTracking()
                .AnyAsync(r => r.Id == staff.RestaurantId && r.IsActive, ct);
            if (!restaurantIsActive)
            {
                return Unauthorized(new { error = "Restaurant is inactive. Contact support." });
            }

            staff.LastLoginAt = DateTimeOffset.UtcNow;
            staff.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            var token = _jwtTokenService.CreateToken(staff);
            return Ok(new LoginResponseDto
            {
                AccessToken = token,
                ExpiresInSeconds = _jwtOptions.ExpiryMinutes * 60,
                User = new AuthUserDto
                {
                    Id = staff.Id,
                    RestaurantId = staff.RestaurantId,
                    FullName = staff.FullName,
                    Email = staff.Email,
                    Role = staff.Role.ToString()
                }
            });
        }

        [HttpGet("me")]
        [Authorize(Policy = AuthConstants.PolicyStaffAny)]
        public async Task<ActionResult<AuthUserDto>> Me(CancellationToken ct)
        {
            var userIdRaw =
                User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                return Unauthorized(new { error = "Invalid user id in token." });
            }

            var staff = await _db.StaffUsers.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
            if (staff is null)
            {
                return Unauthorized(new { error = "User not found." });
            }

            var restaurantIsActive = await _db.Restaurants.AsNoTracking()
                .AnyAsync(r => r.Id == staff.RestaurantId && r.IsActive, ct);
            if (!restaurantIsActive)
            {
                return Unauthorized(new { error = "Restaurant is inactive. Contact support." });
            }

            return Ok(new AuthUserDto
            {
                Id = staff.Id,
                RestaurantId = staff.RestaurantId,
                FullName = staff.FullName,
                Email = staff.Email,
                Role = staff.Role.ToString()
            });
        }
    }
}
