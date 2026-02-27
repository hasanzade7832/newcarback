using System.ComponentModel.DataAnnotations;
using CarAds.Enums;

namespace CarAds.DTOs.Users
{
    public class CreateUserDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Username { get; set; } = null!;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = null!;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = null!;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = null!;

        // ✅ سوپرادمین می‌تواند User یا Admin بسازد (نه SuperAdmin)
        [Required]
        public UserRole Role { get; set; } = UserRole.User;
    }
}
