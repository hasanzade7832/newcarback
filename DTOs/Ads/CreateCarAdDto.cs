using System.ComponentModel.DataAnnotations;
using CarAds.Enums;

namespace CarAds.DTOs.Ads
{
    public class CreateCarAdDto
    {
        [Required]
        public CarAdType Type { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public int Year { get; set; }

        [Required, MaxLength(50)]
        public string Color { get; set; } = null!;

        [Required]
        public int MileageKm { get; set; }

        public int? InsuranceMonths { get; set; }

        public GearboxType Gearbox { get; set; } = GearboxType.None;

        [Required, MaxLength(50)]
        public string ChassisNumber { get; set; } = null!;

        // ✅ شماره تماس مخصوص آگهی
        [Required, MaxLength(20)]
        public string ContactPhone { get; set; } = null!;

        [Required]
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;


    }
}
