namespace CarAds.Models;

public class TelegramMessage
{
    public int Id { get; set; }
    public long MessageId { get; set; }
    public long ChatId { get; set; }
    public string Text { get; set; } = "";
    public string? FromUsername { get; set; }
    public string? FromFirstName { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string TelegramLink { get; set; } = "";
}