using HotelBooking.Api.Contracts;
using System.Text.Json;

namespace HotelBooking.Api.Services;

public sealed class CachedHotelBookingService(
    EfCoreHotelBookingService inner,
    IHotelSearchCache hotelSearchCache,
    ILogger<CachedHotelBookingService> logger) : IHotelBookingService
{
    public HotelDetails? GetHotelById(int hotelId)
    {
        return inner.GetHotelById(hotelId);
    }

    public IReadOnlyList<RoomTypeDetails> GetRoomsByHotelId(int hotelId)
    {
        return inner.GetRoomsByHotelId(hotelId);
    }

    public IReadOnlyList<HotelSearchResult> SearchAvailableRooms(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests,
        string? keyword = null,
        int? minRating = null,
        string? sortBy = null)
    {
        if (hotelSearchCache.TryGet(city, checkIn, checkOut, guests, keyword, minRating, sortBy, out var cachedResults))
        {
            logger.LogInformation(
                "Hotel search cache HIT for {City} {CheckIn} {CheckOut} {Guests} {Keyword} {MinRating} {SortBy}",
                city,
                checkIn,
                checkOut,
                guests,
                keyword,
                minRating,
                sortBy);
            return cachedResults;
        }

        logger.LogInformation(
            "Hotel search cache MISS for {City} {CheckIn} {CheckOut} {Guests} {Keyword} {MinRating} {SortBy}",
            city,
            checkIn,
            checkOut,
            guests,
            keyword,
            minRating,
            sortBy);

        var results = inner.SearchAvailableRooms(city, checkIn, checkOut, guests, keyword, minRating, sortBy);
        hotelSearchCache.Set(city, checkIn, checkOut, guests, keyword, minRating, sortBy, results);
        return results;
    }

    public CreateBookingResult CreateBooking(CreateBookingRequest request, int? userId = null)
    {
        var result = inner.CreateBooking(request, userId);
        if (result.Confirmation is not null)
        {
            hotelSearchCache.ClearSearchResults();
        }

        return result;
    }

    public BookingConfirmation? GetBookingByCode(string bookingCode)
    {
        return inner.GetBookingByCode(bookingCode);
    }

    public BookingConfirmation? GetBookingByPayOsOrderCode(long orderCode, int userId, bool isAdmin = false)
    {
        return inner.GetBookingByPayOsOrderCode(orderCode, userId, isAdmin);
    }

    public IReadOnlyList<BookingConfirmation> GetBookingsForUser(int userId)
    {
        return inner.GetBookingsForUser(userId);
    }

    public BookingConfirmation? ConfirmMockPayment(string bookingCode, int userId, bool isAdmin = false)
    {
        var result = inner.ConfirmMockPayment(bookingCode, userId, isAdmin);
        if (result is not null)
        {
            hotelSearchCache.ClearSearchResults();
        }

        return result;
    }

    public Task<PayOsPaymentLinkResponse?> CreatePayOsPaymentAsync(
        string bookingCode,
        int userId,
        bool isAdmin,
        string returnUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        return inner.CreatePayOsPaymentAsync(bookingCode, userId, isAdmin, returnUrl, cancelUrl, cancellationToken);
    }

    public async Task<PayOsWebhookResult> ProcessPayOsWebhookAsync(
        JsonElement data,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.ProcessPayOsWebhookAsync(data, signature, cancellationToken);
        if (result.Accepted)
        {
            hotelSearchCache.ClearSearchResults();
        }

        return result;
    }

    public BookingConfirmation? CancelBooking(string bookingCode, int userId, bool isAdmin = false)
    {
        var result = inner.CancelBooking(bookingCode, userId, isAdmin);
        if (result is not null)
        {
            hotelSearchCache.ClearSearchResults();
        }

        return result;
    }

    public IReadOnlyList<HotelReviewSummary> GetHotelReviews(int hotelId)
    {
        return inner.GetHotelReviews(hotelId);
    }

    public HotelReviewSummary CreateReview(int hotelId, CreateHotelReviewRequest request, int userId)
    {
        var result = inner.CreateReview(hotelId, request, userId);
        hotelSearchCache.ClearSearchResults();
        return result;
    }
}
