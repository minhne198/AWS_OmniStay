using HotelBooking.Api.Contracts;

namespace HotelBooking.Api.Services;

public interface IHotelBookingService
{
    HotelDetails? GetHotelById(int hotelId);

    IReadOnlyList<RoomTypeDetails> GetRoomsByHotelId(int hotelId);

    IReadOnlyList<HotelSearchResult> SearchAvailableRooms(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests);

    CreateBookingResult CreateBooking(CreateBookingRequest request);

    BookingConfirmation? GetBookingByCode(string bookingCode);
}
