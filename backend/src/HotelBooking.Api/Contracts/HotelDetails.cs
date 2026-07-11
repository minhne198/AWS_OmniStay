namespace HotelBooking.Api.Contracts;

public sealed record HotelDetails(
    int HotelId,
    string Name,
    string City,
    string Address,
    string Description,
    int StarRating,
    string MainImageUrl,
    double AverageRating,
    int ReviewCount);
