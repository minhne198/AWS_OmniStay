namespace HotelBooking.Api.Contracts;

public sealed record RoomTypeDetails(
    int RoomTypeId,
    int HotelId,
    string Name,
    string Description,
    int MaxGuests,
    decimal PricePerNight,
    int TotalRooms,
    string ImageUrl);
