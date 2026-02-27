using CarAds.Data;
using CarAds.DTOs.Bio;
using CarAds.Hubs;
using CarAds.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<CarAdHub> _hub;

        public BioController(AppDbContext context, IHubContext<CarAdHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        [HttpGet("user/{userId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserBio(int userId)
        {
            var items = await _context.UserBioItems
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsAdvanced)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.GroupKey, // ✅ اضافه شد
                    x.IsAdvanced,
                    x.ContactInfo,
                    x.Title,
                    x.Description,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");

            var userId = int.Parse(userIdStr);

            var items = await _context.UserBioItems
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsAdvanced)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.GroupKey, // ✅ اضافه شد
                    x.IsAdvanced,
                    x.ContactInfo,
                    x.Title,
                    x.Description,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateBioItemDto dto)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");

            var userId = int.Parse(userIdStr);

            var isAdvanced = dto.IsAdvanced;

            // ✅ GroupKey: اجباری + ذخیره می‌شود
            var groupKey = (dto.GroupKey ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(groupKey))
                return BadRequest("GroupKey الزامی است");

            // ✅ قوانین قطعی:
            // Advanced: Title + Description(person) + ContactInfo
            // Simple: فقط Description(simpleText)
            var item = new UserBioItem
            {
                UserId = userId,
                GroupKey = groupKey, // ✅ اضافه شد
                IsAdvanced = isAdvanced,

                Title = isAdvanced ? (dto.Title ?? string.Empty).Trim() : string.Empty,
                Description = (dto.Description ?? string.Empty).Trim(),
                ContactInfo = isAdvanced ? (dto.ContactInfo?.Trim()) : null,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserBioItems.Add(item);
            await _context.SaveChangesAsync();

            var payload = new
            {
                item.Id,
                item.UserId,
                item.GroupKey, // ✅ اضافه شد
                item.IsAdvanced,
                item.Title,
                item.Description,
                item.ContactInfo,
                item.CreatedAt,
                item.UpdatedAt
            };

            await _hub.Clients.Group($"User:{userId}").SendAsync("BioItemAdded", payload);
            await _hub.Clients.Group($"Profile:{userId}").SendAsync("BioItemAdded", payload);

            return Ok(new { message = "آیتم بیوگرافی اضافه شد", id = item.Id });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBioItemDto dto)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");

            var userId = int.Parse(userIdStr);

            var item = await _context.UserBioItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return NotFound("آیتم بیوگرافی یافت نشد");

            if (item.UserId != userId)
                return Forbid();

            var isAdvanced = dto.IsAdvanced;

            // ✅ GroupKey: اجباری + در آپدیت هم نگه داشته می‌شود/قابل تغییر است
            var groupKey = (dto.GroupKey ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(groupKey))
                return BadRequest("GroupKey الزامی است");

            item.GroupKey = groupKey; // ✅ اضافه شد
            item.IsAdvanced = isAdvanced;
            item.Title = isAdvanced ? (dto.Title ?? string.Empty).Trim() : string.Empty;
            item.Description = (dto.Description ?? string.Empty).Trim();
            item.ContactInfo = isAdvanced ? (dto.ContactInfo?.Trim()) : null;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var payload = new
            {
                item.Id,
                item.UserId,
                item.GroupKey, // ✅ اضافه شد
                item.IsAdvanced,
                item.Title,
                item.Description,
                item.ContactInfo,
                item.CreatedAt,
                item.UpdatedAt
            };

            await _hub.Clients.Group($"User:{userId}").SendAsync("BioItemUpdated", payload);
            await _hub.Clients.Group($"Profile:{userId}").SendAsync("BioItemUpdated", payload);

            return Ok("آیتم بیوگرافی ویرایش شد");
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");

            var userId = int.Parse(userIdStr);

            var item = await _context.UserBioItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return NotFound("آیتم بیوگرافی یافت نشد");

            if (item.UserId != userId)
                return Forbid();

            _context.UserBioItems.Remove(item);
            await _context.SaveChangesAsync();

            var payload = new { id, userId };

            await _hub.Clients.Group($"User:{userId}").SendAsync("BioItemDeleted", payload);
            await _hub.Clients.Group($"Profile:{userId}").SendAsync("BioItemDeleted", payload);

            return Ok("آیتم بیوگرافی حذف شد");
        }
    }
}
