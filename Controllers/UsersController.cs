using CarAds.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarAds.Models;

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

        // ✅ اندپوینت جدید: دریافت اطلاعات کاربر فعلی
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Invalid token");

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Invalid user ID format");

            return Ok(new { id = userId });
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
                    x.Phone,
                    x.Email,
                    role = x.Role.ToString(),
                    x.CreatedAt,
                    x.ShowroomName,
                    x.Address,
                    x.City
                })
                .FirstOrDefaultAsync();
            if (user == null) return NotFound("کاربر یافت نشد");
            return Ok(user);
        }
    }
}