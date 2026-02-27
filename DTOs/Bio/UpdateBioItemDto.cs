using System.ComponentModel.DataAnnotations;

namespace CarAds.DTOs.Bio
{
    public class UpdateBioItemDto
    {
        // ✅ برای اینکه رکورد همچنان در همان گروه بماند
        [Required, MaxLength(80)]
        public string GroupKey { get; set; } = null!;

        [Required]
        public bool IsAdvanced { get; set; } = true;

        [MaxLength(150)]
        public string? Title { get; set; }

        [Required, MaxLength(2000)]
        public string Description { get; set; } = null!;

        [MaxLength(200)]
        public string? ContactInfo { get; set; }
    }
}
