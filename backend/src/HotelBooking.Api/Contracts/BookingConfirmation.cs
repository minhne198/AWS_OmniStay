namespace HotelBooking.Api.Contracts;

public sealed record BookingConfirmation(
    string BookingCode,
    int RoomTypeId,
    string HotelName,
    string RoomTypeName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    int Guests,
    decimal TotalPrice,
    string Status);
