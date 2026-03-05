// Controllers/CitiesController.cs
using Microsoft.AspNetCore.Mvc;
using CarAds.Data;
using CarAds.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CitiesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// دریافت لیست شهرهای موجود در سیستم
        /// </summary>
        /// <returns>لیست شهرهای منحصر به فرد</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetCities()
        {
            try
            {
                // دریافت شهرهای منحصر به فرد از جدول کاربران
                var cities = await _context.Users
                    .Select(u => u.City)
                    .Distinct()
                    .ToListAsync();

                return Ok(cities);
            }
            catch (Exception ex)
            {
                // ثبت خطای جزئیات در لاگ
                return StatusCode(500, $"خطای سرور: {ex.Message}");
            }
        }
    }
}