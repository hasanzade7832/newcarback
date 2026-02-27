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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            _logger.LogInformation("⏰ Telegram cleanup scheduled in {delay}", delay);
            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var yesterday = DateTime.UtcNow.Date;
                var deleted = await db.TelegramMessages
                    .Where(m => m.ReceivedAt < yesterday)
                    .ExecuteDeleteAsync(stoppingToken);

                _logger.LogInformation("🗑️ Telegram cleanup: {deleted} messages deleted", deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Telegram cleanup failed");
            }
        }
    }
}