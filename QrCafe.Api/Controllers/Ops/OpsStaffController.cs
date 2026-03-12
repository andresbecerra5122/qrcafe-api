using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Domain.Entities;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Controllers.Ops
{
    [ApiController]
    [Route("ops/staff")]
    [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
    public class OpsStaffController : ControllerBase
    {
        private readonly QrCafeDbContext _db;
        private readonly IPasswordHasher _passwordHasher;

        public OpsStaffController(QrCafeDbContext db, IPasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<StaffUserDto>>> Get(CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var isSuperAdmin = User.IsInRole(StaffRole.SuperAdmin.ToString());
            var users = await _db.StaffUsers
                .AsNoTracking()
                .Where(u => u.RestaurantId == restaurantId)
                .Where(u => isSuperAdmin || u.Role != StaffRole.SuperAdmin)
                .OrderBy(u => u.FullName)
                .Select(u => new StaffUserDto
                {
                    Id = u.Id,
                    RestaurantId = u.RestaurantId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync(ct);

            return Ok(users);
        }

        [HttpPatch("me/password")]
        [Authorize(Policy = AuthConstants.PolicyStaffAny)]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordRequestDto req, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var restaurantId = User.GetRestaurantId();
            if (userId == Guid.Empty || restaurantId == Guid.Empty)
            {
                return Unauthorized(new { error = "Invalid user context." });
            }

            if (string.IsNullOrWhiteSpace(req.CurrentPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
            {
                return BadRequest(new { error = "currentPassword and newPassword are required." });
            }

            if (req.NewPassword.Length < 8)
            {
                return BadRequest(new { error = "New password must be at least 8 characters." });
            }

            var entity = await _db.StaffUsers.SingleOrDefaultAsync(
                u => u.Id == userId && u.RestaurantId == restaurantId && u.IsActive,
                ct
            );
            if (entity is null)
            {
                return Unauthorized(new { error = "User not found." });
            }

            if (!_passwordHasher.Verify(req.CurrentPassword, entity.PasswordHash))
            {
                return BadRequest(new { error = "Current password is incorrect." });
            }

            entity.PasswordHash = _passwordHasher.Hash(req.NewPassword);
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<StaffUserDto>> Create([FromBody] CreateStaffUserRequestDto req, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();
            var email = req.Email.Trim().ToLowerInvariant();
            var fullName = req.FullName.Trim();

            if (string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(fullName)
                || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new { error = "fullName, email and password are required." });
            }

            if (!Enum.TryParse<StaffRole>(req.Role, true, out var role))
            {
                return BadRequest(new { error = "Invalid role." });
            }
            if (role == StaffRole.SuperAdmin && !User.IsInRole(StaffRole.SuperAdmin.ToString()))
            {
                return Forbid();
            }

            var exists = await _db.StaffUsers.AnyAsync(
                u => u.RestaurantId == restaurantId && u.Email == email,
                ct
            );
            if (exists)
            {
                return Conflict(new { error = "A staff account with this email already exists." });
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new StaffUser
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                FullName = fullName,
                Email = email,
                PasswordHash = _passwordHasher.Hash(req.Password),
                Role = role,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.StaffUsers.Add(entity);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, new StaffUserDto
            {
                Id = entity.Id,
                RestaurantId = entity.RestaurantId,
                FullName = entity.FullName,
                Email = entity.Email,
                Role = entity.Role.ToString(),
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                LastLoginAt = entity.LastLoginAt
            });
        }

        [HttpPatch("{staffId:guid}")]
        public async Task<ActionResult<StaffUserDto>> Update(Guid staffId, [FromBody] UpdateStaffUserRequestDto req, CancellationToken ct)
        {
            var restaurantId = User.GetRestaurantId();

            var entity = await _db.StaffUsers.SingleOrDefaultAsync(
                u => u.Id == staffId && u.RestaurantId == restaurantId,
                ct
            );
            if (entity is null)
            {
                return NotFound(new { error = "Staff account not found." });
            }

            // Protect platform account from restaurant-level user management actions.
            if (entity.Role == StaffRole.SuperAdmin)
            {
                return BadRequest(new { error = "SuperAdmin account cannot be managed from this endpoint." });
            }

            if (!string.IsNullOrWhiteSpace(req.FullName))
            {
                entity.FullName = req.FullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                entity.PasswordHash = _passwordHasher.Hash(req.Password);
            }

            if (!string.IsNullOrWhiteSpace(req.Role))
            {
                if (!Enum.TryParse<StaffRole>(req.Role, true, out var parsedRole))
                {
                    return BadRequest(new { error = "Invalid role." });
                }
                if (parsedRole == StaffRole.SuperAdmin && !User.IsInRole(StaffRole.SuperAdmin.ToString()))
                {
                    return Forbid();
                }

                entity.Role = parsedRole;
            }

            if (req.IsActive.HasValue)
            {
                entity.IsActive = req.IsActive.Value;
            }

            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Ok(new StaffUserDto
            {
                Id = entity.Id,
                RestaurantId = entity.RestaurantId,
                FullName = entity.FullName,
                Email = entity.Email,
                Role = entity.Role.ToString(),
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                LastLoginAt = entity.LastLoginAt
            });
        }
    }
}
