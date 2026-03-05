using System.ComponentModel.DataAnnotations;

namespace CarAds.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = null!;
        [Required, MaxLength(100)]
        public string LastName { get; set; } = null!;
        [Required, MaxLength(100)]
        public string Username { get; set; } = null!;
        [Required, MaxLength(20)]
        public string Phone { get; set; } = null!;

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        // ✅ اصلاح ۱: Nullable شده و [Required] حذف شده است.
        [MaxLength(150)]
        public string? ShowroomName { get; set; }

        // ✅ اصلاح ۲: Nullable شده و [Required] حذف شده است.
        [MaxLength(250)]
        public string? Address { get; set; }

        //اگر City هم در ویرایش اختیاری است، باید اینجا اضافه شود:
         [MaxLength(100)]
        public string? City { get; set; }
    }
}