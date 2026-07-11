namespace HotelBooking.Api.Models;

public sealed class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string LinkUrl { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
