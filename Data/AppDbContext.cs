using CarAds.Models;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<CarAd> CarAds => Set<CarAd>();
        public DbSet<UserBioItem> UserBioItems => Set<UserBioItem>();
        public DbSet<AdView> AdViews => Set<AdView>(); // ✅
        public DbSet<TelegramMessage> TelegramMessages => Set<TelegramMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // User
            // =========================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FirstName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.LastName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Username)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Phone)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(x => x.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(x => x.PasswordHash)
                      .IsRequired();

                entity.Property(x => x.CreatedAt)
                      .IsRequired();

                entity.HasIndex(x => x.Username).IsUnique();
                entity.HasIndex(x => x.Phone).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();
            });

            // =========================
            // CarAd
            // =========================
            modelBuilder.Entity<CarAd>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(x => x.Description)
                      .IsRequired();

                entity.Property(x => x.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(x => x.Type)
                      .IsRequired();

                entity.Property(x => x.Year)
                      .IsRequired();

                entity.Property(x => x.Color)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(x => x.MileageKm)
                      .IsRequired();

                entity.Property(x => x.InsuranceMonths);

                entity.Property(x => x.Gearbox)
                      .IsRequired();

                entity.Property(x => x.ChassisNumber)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(x => x.ContactPhone)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(x => x.Status)
                      .IsRequired();

                entity.Property(x => x.CreatedAt)
                      .IsRequired();

                entity.Property(x => x.ApprovedAt);
                entity.Property(x => x.RejectedAt);

                // ✅ ViewCount با مقدار پیش‌فرض 0
                entity.Property(x => x.ViewCount)
                      .IsRequired()
                      .HasDefaultValue(0);

                entity.HasOne(x => x.User)
                      .WithMany(u => u.CarAds)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ApprovedByAdmin)
                      .WithMany()
                      .HasForeignKey(x => x.ApprovedByAdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // UserBioItem
            // =========================
            modelBuilder.Entity<UserBioItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.GroupKey)
                      .IsRequired()
                      .HasMaxLength(80);

                entity.Property(x => x.IsAdvanced)
                      .IsRequired();

                entity.Property(x => x.Title)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(x => x.Description)
                      .IsRequired()
                      .HasMaxLength(2000);

                entity.Property(x => x.ContactInfo)
                      .HasMaxLength(200);

                entity.Property(x => x.CreatedAt).IsRequired();
                entity.Property(x => x.UpdatedAt).IsRequired();

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.UserId, x.GroupKey });
            });

            // =========================
            // AdView
            // =========================
            modelBuilder.Entity<AdView>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ViewedAt).IsRequired();

                entity.HasOne(x => x.Ad)
                      .WithMany(a => a.AdViews)
                      .HasForeignKey(x => x.AdId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ✅ Index برای query سریع روی بازدیدهای امروز
                entity.HasIndex(x => x.ViewedAt);
                entity.HasIndex(x => x.AdId);
            });
        }
    }
}