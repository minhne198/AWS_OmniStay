using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.Contracts;

public sealed record HotelReviewSummary(
    int ReviewId,
    int HotelId,
    string BookingCode,
    string ReviewerName,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt);

public sealed record CreateHotelReviewRequest(
    [Required, MaxLength(30)] string BookingCode,
    [Range(1, 5)] int Rating,
    [MaxLength(2_000)] string? Comment);

public sealed record NotificationSummary(
    int NotificationId,
    string Type,
    string Title,
    string Message,
    string LinkUrl,
    bool IsRead,
    DateTimeOffset CreatedAt);

public sealed record OwnerHotelProfileSummary(
    int HotelId,
    string Name,
    string City,
    string MainImageUrl,
    int RoomTypeCount,
    decimal TotalRevenue,
    int BookingCount,
    double AverageRating,
    int ReviewCount);

public sealed record OwnerProfileSummary(
    int OwnerUserId,
    string FullName,
    string Email,
    string AvatarUrl,
    string VerificationStatus,
    decimal TotalRevenue,
    int OwnedHotelCount,
    IReadOnlyList<OwnerHotelProfileSummary> Hotels);
