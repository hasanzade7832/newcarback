using CarAds.Enums;
namespace CarAds.Models
{
    public class CarAd
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public CarAdType Type { get; set; } = CarAdType.UsedSale;
        public int Year { get; set; }
        public string Color { get; set; } = null!;
        public int MileageKm { get; set; }
        public int? InsuranceMonths { get; set; }
        public GearboxType Gearbox { get; set; } = GearboxType.None;
        public string ChassisNumber { get; set; } = null!;
        // ✅ شماره تماس آگهی (با Phone کاربر فرق دارد)
        public string ContactPhone { get; set; } = null!;
        // فعلاً همه آگهی‌ها Approved هستند
        public CarAdStatus Status { get; set; } = CarAdStatus.Approved;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // فعلاً نگه می‌داریم (بعداً اگر خواستی پاکسازی می‌کنیم)
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        // ✅ شمارنده بازدید (ذخیره در دیتابیس)
        public int ViewCount { get; set; } = 0;

        public User User { get; set; } = null!;
        public int? ApprovedByAdminId { get; set; }
        public User? ApprovedByAdmin { get; set; }

        // Navigation
        public ICollection<AdView> AdViews { get; set; } = new List<AdView>();
    }
}