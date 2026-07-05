using HotelBooking.Api.Contracts;

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
        int guests)
    {
        if (hotelSearchCache.TryGet(city, checkIn, checkOut, guests, out var cachedResults))
        {
            logger.LogInformation(
                "Hotel search cache HIT for {City} {CheckIn} {CheckOut} {Guests}",
                city,
                checkIn,
                checkOut,
                guests);
            return cachedResults;
        }

        logger.LogInformation(
            "Hotel search cache MISS for {City} {CheckIn} {CheckOut} {Guests}",
            city,
            checkIn,
            checkOut,
            guests);

        var results = inner.SearchAvailableRooms(city, checkIn, checkOut, guests);
        hotelSearchCache.Set(city, checkIn, checkOut, guests, results);
        return results;
    }

    public CreateBookingResult CreateBooking(CreateBookingRequest request)
    {
        var result = inner.CreateBooking(request);
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
}
