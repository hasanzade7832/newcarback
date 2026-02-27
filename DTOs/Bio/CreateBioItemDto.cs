using System.ComponentModel.DataAnnotations;

namespace CarAds.DTOs.Bio
{
    public class CreateBioItemDto
    {
        // ✅ کلید گروه (از فرانت میاد) برای اینکه ساده و پیشرفته کنار هم ذخیره شوند
        [Required, MaxLength(80)]
        public string GroupKey { get; set; } = null!;

        [Required]
        public bool IsAdvanced { get; set; } = true;

        // در حالت ساده اختیاری
        [MaxLength(150)]
        public string? Title { get; set; }

        [Required, MaxLength(2000)]
        public string Description { get; set; } = null!;

        // در حالت ساده اختیاری
        [MaxLength(200)]
        public string? ContactInfo { get; set; }
    }
}
