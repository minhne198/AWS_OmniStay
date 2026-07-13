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
    string AvatarUrl,
    decimal Balance,
    DateTimeOffset CreatedAt);

public sealed record UpsertUserRequest(
    [Required, MaxLength(200)] string FullName,
    [Required, MaxLength(200)] string Email,
    [MaxLength(200)] string? Password,
    [Required] string Role,
    [Range(0, 1_000_000_000_000)] decimal? Balance = null,
    [MaxLength(500)] string? AvatarUrl = null);

public sealed record AdminHotelSummary(
    int HotelId,
    int? OwnerUserId,
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
    [Range(2_000, 1_000_000_000)] decimal PricePerNight,
    [Range(0, 10_000)] int TotalRooms,
    [Required, MaxLength(500)] string ImageUrl,
    bool IsHidden = false);

public sealed record AdminImageUploadResult(string ImageUrl);

public sealed record RevenuePoint(
    string Label,
    decimal Revenue,
    int BookingCount);

public sealed record DashboardSummary(
    decimal TotalRevenue,
    int BookingCount,
    string MostBookedRoomName,
    int MostBookedRoomBookings,
    int TotalRooms,
    int BookedRooms,
    decimal OccupancyRate,
    IReadOnlyList<RevenuePoint> RevenueByDay,
    IReadOnlyList<RevenuePoint> RevenueByMonth);

public sealed record BalanceTransactionSummary(
    int TransactionId,
    int UserId,
    string UserEmail,
    string UserFullName,
    string? BookingCode,
    decimal Amount,
    decimal BalanceAfter,
    string Type,
    string Description,
    DateTimeOffset CreatedAt);

public sealed record BalanceTopUpRequest(
    [Range(1, 1_000_000_000_000)] decimal Amount,
    [MaxLength(500)] string? Description = null);

public sealed record AdminActivitySummary(
    int NotificationId,
    int UserId,
    string UserEmail,
    string Type,
    string Title,
    string Message,
    string LinkUrl,
    DateTimeOffset CreatedAt);
