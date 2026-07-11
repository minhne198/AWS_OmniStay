namespace HotelBooking.Api.Models;

public sealed class RoomType
{
    public int Id { get; set; }

    public int HotelId { get; set; }

    public Hotel? Hotel { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int MaxGuests { get; set; }

    public decimal PricePerNight { get; set; }

    public int TotalRooms { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsHidden { get; set; }

    public List<Booking> Bookings { get; set; } = [];
}
