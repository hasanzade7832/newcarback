namespace CarAds.Models
{
    public class AdView
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public CarAd Ad { get; set; } = null!;
    }
}