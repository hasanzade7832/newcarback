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
                    u.Email.Contains(q) ||
                    u.Address.Contains(q) ||      // اضافه شد برای سرچ آدرس
                    u.City.Contains(q) ||          // اضافه شد برای سرش شهر
                    u.ShowroomName.Contains(q)      // اضافه شد برای سرچ نام نمایشگاه
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
                    u.Address,
                    u.City,        // ✅ اضافه شد برای نمایش در API
                    u.ShowroomName, // ✅ اضافه شد برای نمایش در API
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
                u.Username == dto.Username ||
                u.Phone == dto.Phone ||
                u.Email == dto.Email
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
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                ShowroomName = dto.ShowroomName?.Trim() ?? string.Empty, // مدیریت مقدار خالی
                Address = dto.Address?.Trim() ?? string.Empty,
                City = dto.City?.Trim() ?? string.Empty                // ✅ مقدار City هم اضافه شد
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password.Trim());
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "کاربر ایجاد شد", id = user.Id });
        }

        // PUT: /api/admin/users/{id}
        [Authorize(Roles = "SuperAdmin")]
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

            // به‌روزرسانی فیلدهای اصلی
            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Username = dto.Username.Trim();
            user.Phone = dto.Phone.Trim();
            user.Email = dto.Email.Trim();

            // === مدیریت فیلدهای جدید (City, Address, ShowroomName) در حالت ویرایش ===
            // اگر مقدار در DTO موجود باشد، آن را به‌روزرسانی می‌کنیم.
            // اگر مقدار null باشد، آن را به null در دیتابیس تبدیل می‌کنیم (برای فیلدهای Nullable).
            // اگر مقدار خالی باشد، می‌توان آن را خالی نگه داشت یا Null کرد (بسته به استراتژی شما).
            // اینجا استراتژی "اگر ارسال شد (حتی اگر خالی) ذخیره شود" را پیاده‌سازی می‌کنیم.

            if (dto.ShowroomName != null)
            {
                user.ShowroomName = dto.ShowroomName.Trim();
            }

            if (dto.Address != null)
            {
                user.Address = dto.Address.Trim();
            }

            if (dto.City != null) // ✅ City هم اضافه شد
            {
                user.City = dto.City.Trim();
            }
            // === پایان تغییرات ===

            await _context.SaveChangesAsync();
            return Ok("کاربر ویرایش شد");
        }
    }
}
