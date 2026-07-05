namespace HotelBooking.Api.Models;

public sealed class Hotel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int StarRating { get; set; }

    public string MainImageUrl { get; set; } = string.Empty;

    public List<RoomType> RoomTypes { get; set; } = [];
}
