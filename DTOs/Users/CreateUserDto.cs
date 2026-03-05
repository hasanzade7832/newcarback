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

        // ✅ تغییر به string? (قابل NULL)
        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; } // ✅ اینجا string? است
        [ MinLength(6), MaxLength(100)]
        public string Password { get; set; } = null!;
        [Required]
        public UserRole Role { get; set; } = UserRole.User;
        [Required, MaxLength(150)]
        public string ShowroomName { get; set; } = null!;
        [Required, MaxLength(250)]
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}