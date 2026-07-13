using HotelBooking.Api.Contracts;
using System.Text.Json;

namespace HotelBooking.Api.Services;

public interface IHotelBookingService
{
    HotelDetails? GetHotelById(int hotelId);

    IReadOnlyList<RoomTypeDetails> GetRoomsByHotelId(int hotelId);

    IReadOnlyList<HotelSearchResult> SearchAvailableRooms(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests,
        string? keyword = null,
        int? minRating = null,
        string? sortBy = null);

    CreateBookingResult CreateBooking(CreateBookingRequest request, int? userId = null);

    BookingConfirmation? GetBookingByCode(string bookingCode);

    BookingConfirmation? GetBookingByPayOsOrderCode(long orderCode, int userId, bool isAdmin = false);

    IReadOnlyList<BookingConfirmation> GetBookingsForUser(int userId);

    BookingConfirmation? ConfirmMockPayment(string bookingCode, int userId, bool isAdmin = false);

    Task<PayOsPaymentLinkResponse?> CreatePayOsPaymentAsync(
        string bookingCode,
        int userId,
        bool isAdmin,
        string returnUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    Task<PayOsWebhookResult> ProcessPayOsWebhookAsync(
        JsonElement data,
        string? signature,
        CancellationToken cancellationToken = default);

    BookingConfirmation? CancelBooking(string bookingCode, int userId, bool isAdmin = false);

    IReadOnlyList<HotelReviewSummary> GetHotelReviews(int hotelId);

    HotelReviewSummary CreateReview(int hotelId, CreateHotelReviewRequest request, int userId);
}
