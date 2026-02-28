using System.Text.Json;
using CarAds.Data;
using CarAds.Hubs;
using CarAds.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<CarAdHub> _hub;

    // گروه مجاز
    private const long AllowedChatId = -1002027760235;

    // سقف پیام‌های نگهداری/نمایش
    private const int MaxKeep = 5000;

    public TelegramController(AppDbContext db, IHubContext<CarAdHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    // ✅ Telegram Webhook
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] JsonElement body)
    {
        try
        {
            if (!body.TryGetProperty("message", out var message))
                return Ok();

            if (!message.TryGetProperty("message_id", out var msgIdEl)) return Ok();
            if (!message.TryGetProperty("chat", out var chat)) return Ok();
            if (!chat.TryGetProperty("id", out var chatIdEl)) return Ok();

            var messageId = msgIdEl.GetInt64();
            var chatId = chatIdEl.GetInt64();

            // فقط پیام‌های گروه مشخص
            if (chatId != AllowedChatId) return Ok();

            // فقط text داشته باشه
            if (!message.TryGetProperty("text", out var textEl)) return Ok();
            var text = textEl.GetString() ?? "";
            if (string.IsNullOrWhiteSpace(text)) return Ok();

            string fromUsername = "";
            string fromFirstName = "";
            if (message.TryGetProperty("from", out var from))
            {
                if (from.TryGetProperty("username", out var un))
                    fromUsername = un.GetString() ?? "";
                if (from.TryGetProperty("first_name", out var fn))
                    fromFirstName = fn.GetString() ?? "";
            }

            // ساخت لینک تلگرام
            // -1002027760235 → 2027760235 (حذف پیشوند -100)
            var channelId = Math.Abs(chatId).ToString();
            if (channelId.StartsWith("100"))
                channelId = channelId.Substring(3);
            var telegramLink = $"https://t.me/c/{channelId}/{messageId}";

            // جلوگیری از duplicate
            var exists = await _db.TelegramMessages
                .AnyAsync(m => m.MessageId == messageId && m.ChatId == chatId);
            if (exists) return Ok();

            var msg = new TelegramMessage
            {
                MessageId = messageId,
                ChatId = chatId,
                Text = text,
                FromUsername = fromUsername,
                FromFirstName = fromFirstName,
                ReceivedAt = DateTime.UtcNow,
                TelegramLink = telegramLink
            };

            _db.TelegramMessages.Add(msg);
            await _db.SaveChangesAsync();

            // ✅ همیشه فقط 5000 پیام آخر نگه دار (تا DB بزرگ نشه) - فقط برای همین گروه
            var overIds = await _db.TelegramMessages
                .Where(m => m.ChatId == AllowedChatId)
                .OrderByDescending(m => m.ReceivedAt)
                .ThenByDescending(m => m.Id)
                .Skip(MaxKeep)
                .Select(m => m.Id)
                .ToListAsync();

            if (overIds.Count > 0)
            {
                _db.TelegramMessages.RemoveRange(overIds.Select(id => new TelegramMessage { Id = id }));
                await _db.SaveChangesAsync();
            }

            // ✅ ارسال real-time به همه کاربران سایت
            // نکته مهم: receivedAt را ISO با Z می‌فرستیم تا مشکل اختلاف زمان بعد از refresh حل شود
            await _hub.Clients.All.SendAsync("TelegramMessageReceived", new
            {
                id = msg.Id,
                messageId = msg.MessageId,
                text = msg.Text,
                fromUsername = msg.FromUsername,
                fromFirstName = msg.FromFirstName,
                receivedAt = msg.ReceivedAt.ToUniversalTime().ToString("O"),
                telegramLink = msg.TelegramLink
            });

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Telegram webhook error: {ex.Message}");
            return Ok(); // همیشه 200 به تلگرام برگردون
        }
    }

    // ✅ آخرین پیام‌ها (5000 تا)
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] int take = 5000)
    {
        if (take < 1) take = 1;
        if (take > MaxKeep) take = MaxKeep;

        // خروجی جدید→قدیم (فرانت خودش مرتب می‌کند)
        var list = await _db.TelegramMessages
            .Where(m => m.ChatId == AllowedChatId)
            .OrderByDescending(m => m.ReceivedAt)
            .ThenByDescending(m => m.Id)
            .Take(take)
            .Select(m => new
            {
                id = m.Id,
                messageId = m.MessageId,
                text = m.Text,
                fromUsername = m.FromUsername,
                fromFirstName = m.FromFirstName,
                receivedAt = m.ReceivedAt.ToUniversalTime().ToString("O"),
                telegramLink = m.TelegramLink
            })
            .ToListAsync();

        return Ok(list);
    }

    // ✅ پیام‌های امروز (قدیمی → جدید) (فعلاً نگه داشتیم)
    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var todayUtc = DateTime.UtcNow.Date;

        var msgs = await _db.TelegramMessages
            .Where(m => m.ChatId == AllowedChatId && m.ReceivedAt >= todayUtc)
            .OrderBy(m => m.ReceivedAt)
            .ThenBy(m => m.Id)
            .Select(m => new
            {
                id = m.Id,
                messageId = m.MessageId,
                text = m.Text,
                fromUsername = m.FromUsername,
                fromFirstName = m.FromFirstName,
                receivedAt = m.ReceivedAt.ToUniversalTime().ToString("O"),
                telegramLink = m.TelegramLink
            })
            .ToListAsync();

        return Ok(msgs);
    }
}