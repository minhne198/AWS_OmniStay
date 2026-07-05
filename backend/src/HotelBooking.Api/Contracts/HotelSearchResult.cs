namespace HotelBooking.Api.Contracts;

public sealed record HotelSearchResult(
    int HotelId,
    int RoomTypeId,
    string HotelName,
    string City,
    string RoomTypeName,
    int MaxGuests,
    decimal PricePerNight,
    int AvailableRooms);
