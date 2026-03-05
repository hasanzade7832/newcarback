using System.ComponentModel.DataAnnotations;

namespace CarAds.Models
{
    public class FlashSettings
    {
        public int Id { get; set; } = 1; // فقط یک رکورد
        public bool IsEnabled { get; set; } = true;
        public int DefaultDurationMinutes { get; set; } = 15;
    }
}