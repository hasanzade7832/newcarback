using CarAds.Data;
using CarAds.DTOs.Ads;
using CarAds.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CarAdsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<CarAdHub> _hubContext;

        public CarAdsController(AppDbContext context, IHubContext<CarAdHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // لیست همه آگهی‌ها برای پنل مدیریت
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ads = await _context.CarAds
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
                    x.Price,
                    x.Gearbox,
                    x.ContactPhone,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(ads);
        }

        // ویرایش آگهی هر کاربر توسط ادمین
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAny(int id, [FromBody] UpdateCarAdDto dto)
        {
            var ad = await _context.CarAds.FirstOrDefaultAsync(x => x.Id == id);
            if (ad == null)
                return NotFound("آگهی یافت نشد");

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
                ad.Type,
                ad.Title,
                ad.Year,
                ad.Color,
                ad.MileageKm,
                ad.Price,
                ad.Gearbox,
                ad.CreatedAt,
                ad.UserId
            };

            await _hubContext.Clients.All.SendAsync("CarAdUpdated", payload);
            await _hubContext.Clients.Group($"User:{ad.UserId}").SendAsync("MyCarAdUpdated", payload);

            return Ok("آگهی ویرایش شد (ادمین)");
        }

        // حذف آگهی هر کاربر توسط ادمین
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAny(int id)
        {
            var ad = await _context.CarAds.FirstOrDefaultAsync(x => x.Id == id);
            if (ad == null)
                return NotFound("آگهی یافت نشد");

            var userId = ad.UserId;

            _context.CarAds.Remove(ad);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("CarAdDeleted", new { adId = id, userId });
            await _hubContext.Clients.Group($"User:{userId}").SendAsync("MyCarAdDeleted", new { adId = id });

            return Ok("آگهی حذف شد (ادمین)");
        }
    }
}
