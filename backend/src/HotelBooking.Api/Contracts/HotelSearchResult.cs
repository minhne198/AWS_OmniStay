namespace HotelBooking.Api.Contracts;

public sealed record HotelSearchResult(
    int HotelId,
    int RoomTypeId,
    string HotelName,
    string City,
    string RoomTypeName,
    string MainImageUrl,
    string RoomImageUrl,
    int MaxGuests,
    decimal PricePerNight,
    int AvailableRooms,
    double AverageRating,
    int ReviewCount);
