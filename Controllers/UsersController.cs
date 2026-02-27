using CarAds.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // پروفایل عمومی کاربر (برای صفحه نمایش کاربر)
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicUser(int id)
        {
            var user = await _context.Users
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.Username,
                    x.Phone,   // اگر نمی‌خوای عمومی باشه، بعداً حذفش می‌کنیم
                    x.Email,   // اگر نمی‌خوای عمومی باشه، بعداً حذفش می‌کنیم
                    role = x.Role.ToString(),
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound("کاربر یافت نشد");
            return Ok(user);
        }
    }
}
