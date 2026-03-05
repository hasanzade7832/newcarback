using CarAds.Enums;
using CarAds.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarAds.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            // اطمینان از به‌روزرسانی دیتابیس (Migration)
            await context.Database.MigrateAsync();

            var superAdminPhone = "09926559671";
            var superAdminPassword = "Admin123!";

            // بررسی وجود کاربر
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Phone == superAdminPhone);

            if (user != null)
            {
                if (user.Username != "superadmin")
                    user.Username = "superadmin";

                if (user.Role != UserRole.SuperAdmin)
                    user.Role = UserRole.SuperAdmin;

                await context.SaveChangesAsync();

                // ✅ تضمین FlashSettings (Id=1)
                var settingsExisting = await context.FlashSettings.FirstOrDefaultAsync(x => x.Id == 1);
                if (settingsExisting == null)
                {
                    context.FlashSettings.Add(new FlashSettings
                    {
                        Id = 1,
                        IsEnabled = true,
                        DefaultDurationMinutes = 15
                    });
                    await context.SaveChangesAsync();
                }

                return;
            }

            // ساخت کاربر جدید
            var newUser = new User
            {
                FirstName = "Super",
                LastName = "Admin",
                Username = "superadmin",
                Phone = superAdminPhone,
                Email = "superadmin@carads.local",
                Role = UserRole.SuperAdmin,
                ShowroomName = "Super Admin",
                Address = "Super Admin Address",
                City = "Tehran",
                CreatedAt = DateTime.UtcNow
            };

            newUser.PasswordHash = hasher.HashPassword(newUser, superAdminPassword);

            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            // ✅ تضمین FlashSettings (Id=1)
            var settings = await context.FlashSettings.FirstOrDefaultAsync(x => x.Id == 1);
            if (settings == null)
            {
                context.FlashSettings.Add(new FlashSettings
                {
                    Id = 1,
                    IsEnabled = true,
                    DefaultDurationMinutes = 15
                });
                await context.SaveChangesAsync();
            }
        }
    }
}