
using CarAds.Data;
using CarAds.DTOs.Ads;
using CarAds.Enums;
using CarAds.Hubs;
using CarAds.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<CarAdHub> _hubContext;
        private readonly IWebHostEnvironment _env;
        public AdsController(
            AppDbContext context,
            IHubContext<CarAdHub> hubContext,
            IWebHostEnvironment env)
        {
            _context = context;
            _hubContext = hubContext;
            _env = env;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAds([FromQuery] CarAdType? type = null)
        {
            var now = DateTime.UtcNow;
            var query = _context.CarAds.AsQueryable();
            if (type.HasValue)
                query = query.Where(x => x.Type == type.Value);
            query = query.Where(x => x.CreatedAt > now.AddHours(-24));
            var items = await query
                .OrderByDescending(x => x.HasFlash && x.FlashEndTime != null && x.FlashEndTime > now)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Type,
                    x.Title,
                    x.Year,
                    x.Color,
                    x.MileageKm,
                    x.Price,
                    x.Gearbox,
                    x.CreatedAt,
                    x.UserId,
                    x.InsuranceMonths,
                    x.ChassisNumber,
                    x.ContactPhone,
                    x.Description,
                    x.ViewCount,
                    x.HasFlash,
                    x.FlashEndTime
                })
                .ToListAsync();
            return Ok(items);
        }
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAd(int id)
        {
            var ad = await _context.CarAds
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Type,
                    x.Title,
                    x.Year,
                    x.Color,
                    x.MileageKm,
                    x.InsuranceMonths,
                    x.Gearbox,
                    x.ChassisNumber,
                    x.ContactPhone,
                    x.Price,
                    x.Description,
                    x.CreatedAt,
                    x.UserId,
                    x.ViewCount,
                    x.HasFlash,
                    x.FlashEndTime
                })
                .FirstOrDefaultAsync();
            if (ad == null)
                return NotFound("آگهی یافت نشد");
            return Ok(ad);
        }
        [HttpPost("{id:int}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> RecordView(int id)
        {
            var ad = await _context.CarAds.FirstOrDefaultAsync(x => x.Id == id);
            if (ad == null)
                return NotFound("آگهی یافت نشد");
            ad.ViewCount++;
            _context.AdViews.Add(new AdView
            {
                AdId = id,
                ViewedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            await _hubContext.Clients
                .Group($"User:{ad.UserId}")
                .SendAsync("AdViewUpdated", new
                {
                    adId = id,
                    viewCount = ad.ViewCount
                });
            var todayViews = await _context.AdViews
                .Where(v => v.ViewedAt.Date == DateTime.UtcNow.Date)
                .CountAsync();
            await _hubContext.Clients.All.SendAsync("TodayViewsUpdated", todayViews);
            return Ok(new { viewCount = ad.ViewCount });
        }
        [HttpGet("stats/today")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTodayStats()
        {
            var today = DateTime.UtcNow.Date;
            var todayViews = await _context.AdViews
                .Where(v => v.ViewedAt >= today)
                .CountAsync();
            return Ok(new { todayViews });
        }
        [HttpPost("{id:int}/flash")]
        [Authorize]
        public async Task<IActionResult> ActivateFlash(int id)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr)) return Unauthorized("توکن معتبر نیست");
            var userId = int.Parse(userIdStr);
            var ad = await _context.CarAds.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (ad == null) return NotFound("آگهی یافت نشد یا متعلق به شما نیست.");
            var settings = await _context.FlashSettings.FindAsync(1);
            if (settings == null)
            {
                settings = new FlashSettings { Id = 1, IsEnabled = true, DefaultDurationMinutes = 15 };
                _context.FlashSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            if (!settings.IsEnabled)
                return BadRequest("سیستم فوروارد غیرفعال است.");
            if (ad.HasFlash && ad.FlashEndTime.HasValue && ad.FlashEndTime.Value > DateTime.UtcNow)
                return BadRequest("این آگهی قبلاً فوروارد شده و هنوز فعال است.");
            var duration = settings.DefaultDurationMinutes;
            var flashEndTime = DateTime.UtcNow.AddMinutes(duration);
            ad.HasFlash = true;
            ad.FlashEndTime = flashEndTime;
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("FlashStatusUpdated", new
            {
                AdId = ad.Id,
                HasFlash = ad.HasFlash,
                EndTime = ad.FlashEndTime
            });
            return Ok(new
            {
                message = $"فوروارد فعال شد. تا {duration} دقیقه.",
                EndTime = flashEndTime
            });
        }
        [HttpPost]
        [Authorize]
        // اگر قصد ارسال فایل عکس ندارید، RequestFormLimits را هم می‌توانید حذف کنید
        public async Task<IActionResult> Create([FromBody] CreateCarAdDto dto)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");
            var userId = int.Parse(userIdStr);
            var ad = new CarAd
            {
                UserId = userId,
                Type = dto.Type,
                Title = dto.Title,
                Year = dto.Year,
                Color = dto.Color,
                MileageKm = dto.MileageKm,
                InsuranceMonths = dto.InsuranceMonths,
                Gearbox = dto.Gearbox,
                ChassisNumber = dto.ChassisNumber,
                ContactPhone = dto.ContactPhone,
                Price = dto.Price,
                Description = dto.Description ?? string.Empty,
                Status = CarAds.Enums.CarAdStatus.Approved,
                CreatedAt = DateTime.UtcNow,
                ApprovedAt = DateTime.UtcNow,
                ApprovedByAdminId = null,
                ViewCount = 0,
                HasFlash = false
            };
            _context.CarAds.Add(ad);
            await _context.SaveChangesAsync();
            var publicPayload = new
            {
                ad.Id,
                ad.Type,
                ad.Title,
                ad.Year,
                ad.Color,
                ad.MileageKm,
                ad.Price,
                ad.Gearbox,
                ad.CreatedAt,
                ad.UserId,
                ad.InsuranceMonths,
                ad.ChassisNumber,
                ad.ContactPhone,
                ad.Description,
                ad.ViewCount,
                ad.HasFlash,
                ad.FlashEndTime
            };
            await _hubContext.Clients.All.SendAsync("CarAdApproved", publicPayload);
            await _hubContext.Clients.Group($"User:{userId}").SendAsync("CarAdCreatedForUser", publicPayload);
            return Ok(new { message = "آگهی ثبت شد.", adId = ad.Id });
        }
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");
            var userId = int.Parse(userIdStr);
            var ads = await _context.CarAds
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.HasFlash && x.FlashEndTime != null && x.FlashEndTime > DateTime.UtcNow)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.Type,
                    x.Title,
                    x.Year,
                    x.Color,
                    x.MileageKm,
                    x.InsuranceMonths,
                    x.ChassisNumber,
                    x.ContactPhone,
                    x.Description,
                    x.Price,
                    x.Gearbox,
                    x.CreatedAt,
                    x.ViewCount,
                    x.HasFlash,
                    x.FlashEndTime
                })
                .ToListAsync();
            return Ok(ads);
        }
        [HttpPut("{id:int}")]
        [Authorize]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCarAdDto dto)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");
            var userId = int.Parse(userIdStr);
            var ad = await _context.CarAds.FirstOrDefaultAsync(x => x.Id == id);
            if (ad == null)
                return NotFound("آگهی یافت نشد");
            if (ad.UserId != userId)
                return Forbid();
            ad.Type = dto.Type;
            ad.Title = dto.Title;
            ad.Year = dto.Year;
            ad.Color = dto.Color;
            ad.MileageKm = dto.MileageKm;
            ad.InsuranceMonths = dto.InsuranceMonths;
            ad.Gearbox = dto.Gearbox;
            ad.ChassisNumber = dto.ChassisNumber;
            ad.ContactPhone = dto.ContactPhone;
            ad.Price = dto.Price;
            ad.Description = dto.Description ?? string.Empty;
            await _context.SaveChangesAsync();
            var payload = new
            {
                ad.Id,
                ad.UserId,
                ad.Type,
                ad.Title,
                ad.Year,
                ad.Color,
                ad.MileageKm,
                ad.InsuranceMonths,
                ad.Gearbox,
                ad.ChassisNumber,
                ad.ContactPhone,
                ad.Price,
                ad.Description,
                ad.CreatedAt,
                ad.ViewCount,
                ad.HasFlash,
                ad.FlashEndTime
            };
            await _hubContext.Clients.All.SendAsync("CarAdUpdated", payload);
            await _hubContext.Clients.Group($"User:{ad.UserId}").SendAsync("MyCarAdUpdated", payload);
            return Ok("آگهی ویرایش شد");
        }
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");
            var userId = int.Parse(userIdStr);
            var ad = await _context.CarAds.FirstOrDefaultAsync(x => x.Id == id);
            if (ad == null)
                return NotFound("آگهی یافت نشد");
            if (ad.UserId != userId)
                return Forbid();
            _context.CarAds.Remove(ad);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("CarAdDeleted", new { adId = id, userId });
            await _hubContext.Clients.Group($"User:{userId}").SendAsync("MyCarAdDeleted", new { adId = id });
            return Ok("آگهی حذف شد");
        }
        [HttpGet("flash-settings")]
        [Authorize]
        public async Task<IActionResult> GetFlashSettings()
        {
            var settings = await _context.FlashSettings.FindAsync(1);
            if (settings == null)
            {
                settings = new FlashSettings
                {
                    Id = 1,
                    IsEnabled = true,
                    DefaultDurationMinutes = 15
                };
                _context.FlashSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return Ok(new
            {
                isEnabled = settings.IsEnabled,
                defaultDurationMinutes = settings.DefaultDurationMinutes
            });
        }
        [HttpPost("flash-settings")]
        [Authorize]
        public async Task<IActionResult> UpdateFlashSettings([FromBody] FlashSettingsDto dto)
        {
            var settings = await _context.FlashSettings.FindAsync(1);
            if (settings == null)
            {
                settings = new FlashSettings { Id = 1 };
                _context.FlashSettings.Add(settings);
            }
            settings.IsEnabled = dto.IsEnabled;
            settings.DefaultDurationMinutes = dto.DefaultDurationMinutes;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
    public class FlashSettingsDto
    {
        public bool IsEnabled { get; set; }
        public int DefaultDurationMinutes { get; set; }
    }
}
