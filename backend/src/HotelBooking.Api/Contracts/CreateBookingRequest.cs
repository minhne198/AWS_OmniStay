using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.Contracts;

public sealed record CreateBookingRequest(
    [Range(1, int.MaxValue)] int RoomTypeId,
    [Required, MaxLength(200)] string GuestName,
    [Required, EmailAddress, MaxLength(200)] string GuestEmail,
    DateOnly CheckIn,
    DateOnly CheckOut,
    [Range(1, 20)] int Guests);
