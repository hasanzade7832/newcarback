using CarAds.Data;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Services
{
    public class AdCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AdCleanupService> _logger;

        public AdCleanupService(IServiceScopeFactory scopeFactory, ILogger<AdCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AdCleanupService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;
                    var cutoff = now.AddHours(-24);

                    // 1) حذف واقعی آگهی‌های قدیمی
                    var expiredAds = await db.CarAds
                        .Where(a => a.CreatedAt <= cutoff)
                        .ToListAsync(stoppingToken);

                    if (expiredAds.Count > 0)
                    {
                        db.CarAds.RemoveRange(expiredAds);
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Deleted {Count} expired ads.", expiredAds.Count);
                    }

                    // 2) ریست فورواردهای تمام‌شده (اختیاری اما مفید)
                    var expiredFlashAds = await db.CarAds
                        .Where(a => a.HasFlash && a.FlashEndTime != null && a.FlashEndTime <= now)
                        .ToListAsync(stoppingToken);

                    if (expiredFlashAds.Count > 0)
                    {
                        foreach (var ad in expiredFlashAds)
                        {
                            ad.HasFlash = false;
                            ad.FlashEndTime = null;
                        }
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Reset flash on {Count} ads.", expiredFlashAds.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AdCleanupService error.");
                }

                // هر 5 دقیقه
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("AdCleanupService stopped.");
        }
    }
}