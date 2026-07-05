using System.Globalization;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public sealed class EfCoreHotelBookingService(HotelBookingDbContext dbContext) : IHotelBookingService
{
    private const string ConfirmedStatus = "Confirmed";

    public HotelDetails? GetHotelById(int hotelId)
    {
        return dbContext.Hotels
            .AsNoTracking()
            .Where(hotel => hotel.Id == hotelId)
            .Select(hotel => new HotelDetails(
                hotel.Id,
                hotel.Name,
                hotel.City,
                hotel.Address,
                hotel.Description,
                hotel.StarRating,
                hotel.MainImageUrl))
            .SingleOrDefault();
    }

    public IReadOnlyList<RoomTypeDetails> GetRoomsByHotelId(int hotelId)
    {
        return dbContext.RoomTypes
            .AsNoTracking()
            .Where(roomType => roomType.HotelId == hotelId)
            .OrderBy(roomType => roomType.PricePerNight)
            .Select(roomType => new RoomTypeDetails(
                roomType.Id,
                roomType.HotelId,
                roomType.Name,
                roomType.Description,
                roomType.MaxGuests,
                roomType.PricePerNight,
                roomType.TotalRooms,
                roomType.ImageUrl))
            .ToArray();
    }

    public IReadOnlyList<HotelSearchResult> SearchAvailableRooms(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests)
    {
        if (string.IsNullOrWhiteSpace(city) || checkOut <= checkIn || guests <= 0)
        {
            return [];
        }

        var normalizedCity = city.Trim();

        var results = dbContext.RoomTypes
            .AsNoTracking()
            .Where(roomType => roomType.Hotel != null)
            .Where(roomType => roomType.Hotel!.City == normalizedCity)
            .Where(roomType => roomType.MaxGuests >= guests)
            .Select(roomType => new
            {
                RoomType = roomType,
                Hotel = roomType.Hotel!,
                BookedRooms = dbContext.Bookings.Count(booking =>
                    booking.RoomTypeId == roomType.Id
                    && booking.Status == ConfirmedStatus
                    && booking.CheckIn < checkOut
                    && booking.CheckOut > checkIn)
            })
            .OrderBy(item => item.RoomType.PricePerNight)
            .ToArray();

        return results
            .Select(item => new
            {
                item.RoomType,
                item.Hotel,
                AvailableRooms = item.RoomType.TotalRooms - item.BookedRooms
            })
            .Where(item => item.AvailableRooms > 0)
            .Select(item => new HotelSearchResult(
                HotelId: item.Hotel.Id,
                RoomTypeId: item.RoomType.Id,
                HotelName: item.Hotel.Name,
                City: item.Hotel.City,
                RoomTypeName: item.RoomType.Name,
                MaxGuests: item.RoomType.MaxGuests,
                PricePerNight: item.RoomType.PricePerNight,
                AvailableRooms: item.AvailableRooms))
            .ToArray();
    }

    public CreateBookingResult CreateBooking(CreateBookingRequest request)
    {
        if (request.CheckOut <= request.CheckIn)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.InvalidDates,
                "Check-out date must be after check-in date.");
        }

        var roomType = dbContext.RoomTypes
            .Include(room => room.Hotel)
            .SingleOrDefault(room => room.Id == request.RoomTypeId);

        if (roomType is null)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.RoomTypeNotFound,
                "Room type was not found.");
        }

        if (request.Guests <= 0 || request.Guests > roomType.MaxGuests)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.InvalidGuests,
                "Guest count is invalid for this room type.");
        }

        var bookedRooms = dbContext.Bookings.Count(booking =>
            booking.RoomTypeId == roomType.Id
            && booking.Status == ConfirmedStatus
            && booking.CheckIn < request.CheckOut
            && booking.CheckOut > request.CheckIn);

        if (roomType.TotalRooms - bookedRooms <= 0)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.SoldOut,
                "This room type is sold out for the selected dates.");
        }

        var nights = CalculateNights(request.CheckIn, request.CheckOut);
        var booking = new Booking
        {
            BookingCode = CreateBookingCode(request.CheckIn),
            RoomTypeId = roomType.Id,
            GuestName = request.GuestName.Trim(),
            GuestEmail = request.GuestEmail.Trim(),
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Guests = request.Guests,
            TotalPrice = nights * roomType.PricePerNight,
            Status = ConfirmedStatus,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Bookings.Add(booking);
        dbContext.SaveChanges();

        return CreateBookingResult.Success(ToConfirmation(booking, roomType));
    }

    public BookingConfirmation? GetBookingByCode(string bookingCode)
    {
        if (string.IsNullOrWhiteSpace(bookingCode))
        {
            return null;
        }

        var booking = dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .SingleOrDefault(item => item.BookingCode == bookingCode.Trim());

        return booking?.RoomType is null ? null : ToConfirmation(booking, booking.RoomType);
    }

    private string CreateBookingCode(DateOnly checkIn)
    {
        var datePart = checkIn.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var bookingCount = dbContext.Bookings.Count(booking => booking.CheckIn == checkIn);
        return $"BK{datePart}-{bookingCount + 1:000}";
    }

    private static BookingConfirmation ToConfirmation(Booking booking, RoomType roomType)
    {
        var hotel = roomType.Hotel
            ?? throw new InvalidOperationException("Room type must include its hotel.");

        return new BookingConfirmation(
            BookingCode: booking.BookingCode,
            RoomTypeId: booking.RoomTypeId,
            HotelName: hotel.Name,
            RoomTypeName: roomType.Name,
            CheckIn: booking.CheckIn,
            CheckOut: booking.CheckOut,
            Nights: CalculateNights(booking.CheckIn, booking.CheckOut),
            Guests: booking.Guests,
            TotalPrice: booking.TotalPrice,
            Status: booking.Status);
    }

    private static int CalculateNights(DateOnly checkIn, DateOnly checkOut)
    {
        return checkOut.DayNumber - checkIn.DayNumber;
    }
}
