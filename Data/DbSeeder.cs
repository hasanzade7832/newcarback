using CarAds.Enums;
using CarAds.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            await context.Database.MigrateAsync();

            var superAdminPhone = "09926559671";
            var superAdminPassword = "Admin123!";

            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Phone == superAdminPhone);

            if (user != null)
            {
                if (user.Role != UserRole.SuperAdmin)
                    user.Role = UserRole.SuperAdmin;

                await context.SaveChangesAsync();
                return;
            }

            var newUser = new User
            {
                FirstName = "Super",
                LastName = "Admin",
                Username = "superadmin",
                Phone = superAdminPhone,
                Email = "superadmin@carads.local",
                Role = UserRole.SuperAdmin,
                CreatedAt = DateTime.UtcNow
            };

            newUser.PasswordHash = hasher.HashPassword(newUser, superAdminPassword);

            context.Users.Add(newUser);
            await context.SaveChangesAsync();
        }
    }
}
