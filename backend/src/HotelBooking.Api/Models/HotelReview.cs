namespace HotelBooking.Api.Models;

public sealed class HotelReview
{
    public int Id { get; set; }

    public int HotelId { get; set; }

    public Hotel? Hotel { get; set; }

    public int BookingId { get; set; }

    public Booking? Booking { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
