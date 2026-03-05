using System.ComponentModel.DataAnnotations;

namespace CarAds.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Username { get; set; } = null!;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = null!;

        [MaxLength(200)] // ✅ حذف [Required] (ایمیل اختیاری شد)
        public string? Email { get; set; } // ✅ این فیلد اکنون اختیاری است

        [MinLength(6), MaxLength(100)]
        public string Password { get; set; } = null!;

        [Required, MaxLength(250)] // ✅ این خط اضافه شد
        public string Address { get; set; } = null!; // ✅ این خط اضافه شد

        [Required, MaxLength(150)] // ✅ این خط اضافه شد
        public string ShowroomName { get; set; } = null!;

        [Required, MaxLength(100)] // ✅ فیلد جدید
        public string City { get; set; } = null!; // ✅ الزامی
    }
}