namespace CarAds.Models
{
    public class UserBioItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // ✅ جدید: کلید گروه برای اینکه پیشرفته و ساده تو یک باکس باشند
        // هر دو رکورد (advanced + simple) با یک GroupKey مشترک ذخیره می‌شوند.
        public string GroupKey { get; set; } = string.Empty;

        // ✅ حالت پیشرفته/ساده
        public bool IsAdvanced { get; set; } = true;

        // ✅ عنوان فقط برای پیشرفته
        public string Title { get; set; } = string.Empty;

        // ✅ پیشرفته: نام شخص (یا توضیح اصلی پیشرفته)
        // ✅ ساده: متن ساده کامل
        public string Description { get; set; } = null!;

        // ✅ فقط برای پیشرفته
        public string? ContactInfo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
    }
}
