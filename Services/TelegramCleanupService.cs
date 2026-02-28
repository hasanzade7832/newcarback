using CarAds.Data;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Services;

public class TelegramCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramCleanupService> _logger;

    public TelegramCleanupService(IServiceScopeFactory scopeFactory,
        ILogger<TelegramCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    private static TimeZoneInfo GetTehranTimeZone()
    {
        // روی لینوکس/اوبونتو معمولاً همین ID درست است
        return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tehranTz = GetTehranTimeZone();

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = DateTime.UtcNow;
            var nowTehran = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tehranTz);

            // نیمه شب تهران
            var nextMidnightTehran = nowTehran.Date.AddDays(1);
            var nextMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(nextMidnightTehran, tehranTz);

            var delay = nextMidnightUtc - nowUtc;
            if (delay < TimeSpan.Zero) delay = TimeSpan.FromSeconds(5);

            _logger.LogInformation("⏰ Telegram cleanup scheduled for Tehran 00:00. Delay={delay}", delay);
            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // مرز شروع امروز تهران -> UTC
                var todayTehran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tehranTz).Date;
                var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(todayTehran, tehranTz);

                var deleted = await db.TelegramMessages
                    .Where(m => m.ReceivedAt < todayStartUtc)
                    .ExecuteDeleteAsync(stoppingToken);

                _logger.LogInformation("🗑️ Telegram cleanup (Tehran): {deleted} messages deleted", deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Telegram cleanup failed");
            }
        }
    }
}