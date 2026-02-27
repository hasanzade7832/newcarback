using CarAds.Data;
using CarAds.DTOs.Users;
using CarAds.Enums;
using CarAds.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "SuperAdmin")]
    public class UsersAdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsersAdminController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: /api/admin/users?q=
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? q = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u =>
                    u.FirstName.Contains(q) ||
                    u.LastName.Contains(q) ||
                    u.Username.Contains(q) ||
                    u.Phone.Contains(q) ||
                    u.Email.Contains(q)
                );
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Username,
                    u.Phone,
                    u.Email,
                    role = u.Role.ToString(),
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // POST: /api/admin/users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            dto.Username = dto.Username.Trim();
            dto.Phone = dto.Phone.Trim();
            dto.Email = dto.Email.Trim();

            // جلوگیری از ساخت SuperAdmin از طریق API
            if (dto.Role == UserRole.SuperAdmin)
                return BadRequest("ایجاد SuperAdmin مجاز نیست.");

            // چک تکراری‌ها
            var exists = await _context.Users.AnyAsync(u =>
                u.Username == dto.Username || u.Phone == dto.Phone || u.Email == dto.Email
            );

            if (exists)
                return BadRequest("نام کاربری یا شماره یا ایمیل تکراری است.");

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Username = dto.Username,
                Phone = dto.Phone,
                Email = dto.Email,
                Role = dto.Role, // User/Admin
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password.Trim());

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "کاربر ایجاد شد", id = user.Id });
        }

        // PUT: /api/admin/users/{id}
        // ✅ فقط اطلاعات اصلی را ویرایش می‌کند (Role داخل UpdateUserDto نیست)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("کاربر یافت نشد");

            // جلوگیری از دستکاری SuperAdmin
            if (user.Role == UserRole.SuperAdmin)
                return BadRequest("ویرایش SuperAdmin مجاز نیست.");

            // چک تکراری‌ها بجز خودش
            var exists = await _context.Users.AnyAsync(u =>
                u.Id != id && (u.Username == dto.Username || u.Phone == dto.Phone || u.Email == dto.Email)
            );
            if (exists)
                return BadRequest("نام کاربری یا شماره یا ایمیل تکراری است.");

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Username = dto.Username.Trim();
            user.Phone = dto.Phone.Trim();
            user.Email = dto.Email.Trim();

            await _context.SaveChangesAsync();
            return Ok("کاربر ویرایش شد");
        }

        // PUT: /api/admin/users/{id}/role
        // ✅ (اختیاری ولی کاربردی) تغییر نقش فقط توسط سوپرادمین
        public class UpdateUserRoleDto
        {
            public UserRole Role { get; set; } = UserRole.User;
        }

        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("کاربر یافت نشد");

            if (user.Role == UserRole.SuperAdmin)
                return BadRequest("تغییر نقش SuperAdmin مجاز نیست.");

            if (dto.Role == UserRole.SuperAdmin)
                return BadRequest("قرار دادن نقش SuperAdmin مجاز نیست.");

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            return Ok("نقش کاربر تغییر کرد");
        }

        // DELETE: /api/admin/users/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserIdStr = User.FindFirst("userId")?.Value;
            int.TryParse(currentUserIdStr, out var currentUserId);

            if (currentUserId == id)
                return BadRequest("امکان حذف خودتان وجود ندارد.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("کاربر یافت نشد");

            if (user.Role == UserRole.SuperAdmin)
                return BadRequest("حذف SuperAdmin مجاز نیست.");

            // اگر حذف وابستگی‌ها لازم داری:
            var bios = await _context.UserBioItems.Where(b => b.UserId == id).ToListAsync();
            if (bios.Count > 0) _context.UserBioItems.RemoveRange(bios);

            var ads = await _context.CarAds.Where(a => a.UserId == id).ToListAsync();
            if (ads.Count > 0) _context.CarAds.RemoveRange(ads);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("کاربر حذف شد");
        }
    }
}
