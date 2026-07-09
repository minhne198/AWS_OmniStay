using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.Contracts;

public sealed record AdminBookingSummary(
    string BookingCode,
    string GuestName,
    string GuestEmail,
    string? UserEmail,
    string HotelName,
    string RoomTypeName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests,
    decimal TotalPrice,
    string Status,
    string PaymentStatus,
    DateTimeOffset CreatedAt);

public sealed record AdminUserSummary(
    int UserId,
    string FullName,
    string Email,
    string Role,
    DateTimeOffset CreatedAt);

public sealed record UpsertUserRequest(
    [Required, MaxLength(200)] string FullName,
    [Required, MaxLength(200)] string Email,
    [Required, MaxLength(200)] string? Password,
    [Required] string Role);

public sealed record AdminHotelSummary(
    int HotelId,
    string Name,
    string City,
    string Address,
    string Description,
    int StarRating,
    string MainImageUrl,
    int RoomTypeCount);

public sealed record UpsertHotelRequest(
    [Required, MaxLength(200)] string Name,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(500)] string Address,
    [Required, MaxLength(2000)] string Description,
    [Range(1, 5)] int StarRating,
    [Required, MaxLength(500)] string MainImageUrl);

public sealed record UpsertRoomTypeRequest(
    [Range(1, int.MaxValue)] int HotelId,
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(2000)] string Description,
    [Range(1, 20)] int MaxGuests,
    [Range(0, 1_000_000_000)] decimal PricePerNight,
    [Range(0, 10_000)] int TotalRooms,
    [Required, MaxLength(500)] string ImageUrl);
