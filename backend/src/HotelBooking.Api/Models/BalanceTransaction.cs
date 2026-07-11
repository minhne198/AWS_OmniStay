namespace HotelBooking.Api.Models;

public sealed class BalanceTransaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public int? BookingId { get; set; }

    public Booking? Booking { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
