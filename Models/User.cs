using System.ComponentModel.DataAnnotations;
using CarAds.Enums;

namespace CarAds.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ تغییر: Nullable شده و [Required] حذف شد (برای حالت ویرایش)
        [MaxLength(150)]
        public string? ShowroomName { get; set; }

        // ✅ تغییر: Nullable شده و [Required] حذف شد (برای حالت ویرایش)
        [MaxLength(250)]
        public string? Address { get; set; }

        public ICollection<CarAd> CarAds { get; set; } = new List<CarAd>();

        // ✅ تغییر: Nullable شده و [Required] حذف شد (برای حالت ویرایش)
        [MaxLength(100)]
        public string? City { get; set; }
    }
}
