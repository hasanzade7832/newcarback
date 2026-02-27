using CarAds.Data;
using CarAds.DTOs.Ads;
using CarAds.Enums;
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
    public class AdsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<CarAdHub> _hubContext;

        public AdsController(AppDbContext context, IHubContext<CarAdHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // =========================
        // لیست عمومی آگهی‌ها
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAds([FromQuery] CarAdType? type = null)
        {
            var q = _context.CarAds.AsQueryable();

            if (type.HasValue)
                q = q.Where(x => x.Type == type.Value);

            var items = await q
                .OrderByDescending(x => x.CreatedAt)
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
                    x.ViewCount // ✅
                })
                .ToListAsync();

            return Ok(items);
        }

        // =========================
        // جزئیات عمومی یک آگهی
        // =========================
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
                    x.ViewCount // ✅
                })
                .FirstOrDefaultAsync();

            if (ad == null)
                return NotFound("آگهی یافت نشد");

            return Ok(ad);
        }

        // =========================
        // ✅ ثبت بازدید آگهی (در دیتابیس)
        // =========================
        [HttpPost("{id:int}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> RecordView(int id)
        {
            var ad = await _context.CarAds.FirstOrDefaultAsync(x => x.Id == id);
            if (ad == null)
                return NotFound("آگهی یافت نشد");

            // افزایش شمارنده
            ad.ViewCount++;

            // ذخیره سابقه بازدید برای آمار روزانه
            _context.AdViews.Add(new AdView
            {
                AdId = id,
                ViewedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // ✅ اطلاع‌رسانی real-time به داشبورد صاحب آگهی
            await _hubContext.Clients
                .Group($"User:{ad.UserId}")
                .SendAsync("AdViewUpdated", new
                {
                    adId = id,
                    viewCount = ad.ViewCount
                });

            // ✅ اطلاع‌رسانی آمار امروز به همه
            var todayViews = await _context.AdViews
                .Where(v => v.ViewedAt.Date == DateTime.UtcNow.Date)
                .CountAsync();

            await _hubContext.Clients.All.SendAsync("TodayViewsUpdated", todayViews);

            return Ok(new { viewCount = ad.ViewCount });
        }

        // =========================
        // ✅ آمار: بازدید امروز
        // =========================
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

        // =========================
        // ثبت آگهی جدید (کاربر لاگین)
        // =========================
        [HttpPost]
        [Authorize]
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
                Status = CarAdStatus.Approved,
                CreatedAt = DateTime.UtcNow,
                ApprovedAt = DateTime.UtcNow,
                ApprovedByAdminId = null,
                ViewCount = 0 // ✅
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
                ad.ViewCount // ✅
            };

            await _hubContext.Clients.All.SendAsync("CarAdApproved", publicPayload);

            var userPayload = new
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
                ad.ViewCount // ✅
            };

            await _hubContext.Clients.Group($"User:{userId}").SendAsync("CarAdCreatedForUser", userPayload);

            return Ok(new { message = "آگهی ثبت شد.", adId = ad.Id });
        }

        // =========================
        // آگهی‌های خودم
        // =========================
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
                .OrderByDescending(x => x.CreatedAt)
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
                    x.ViewCount // ✅
                })
                .ToListAsync();

            return Ok(ads);
        }

        // =========================
        // ویرایش آگهی (فقط صاحب آگهی)
        // =========================
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCarAdDto dto)
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
                ad.ViewCount // ✅
            };

            await _hubContext.Clients.All.SendAsync("CarAdUpdated", payload);
            await _hubContext.Clients.Group($"User:{ad.UserId}").SendAsync("MyCarAdUpdated", payload);

            return Ok("آگهی ویرایش شد");
        }

        // =========================
        // حذف آگهی (فقط صاحب آگهی)
        // =========================
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
    }
}