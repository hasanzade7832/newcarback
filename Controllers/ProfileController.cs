using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ هر کسی که لاگین است
    public class ProfileController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst("userId")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "پروفایل کاربر",
                userId,
                role
            });
        }
    }
}
