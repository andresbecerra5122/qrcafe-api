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
            var users = await _db.StaffUsers
                .AsNoTracking()
                .Where(u => u.RestaurantId == restaurantId)
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
