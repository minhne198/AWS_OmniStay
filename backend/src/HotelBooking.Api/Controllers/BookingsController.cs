using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(IHotelBookingService hotelBookingService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<BookingConfirmation>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<BookingConfirmation> Create(CreateBookingRequest request)
    {
        var result = hotelBookingService.CreateBooking(request);
        if (result.Confirmation is not null)
        {
            return CreatedAtAction(
                nameof(GetByCode),
                new { code = result.Confirmation.BookingCode },
                result.Confirmation);
        }

        return result.FailureReason switch
        {
            BookingFailureReason.RoomTypeNotFound => NotFound(new { error = result.Message }),
            BookingFailureReason.SoldOut => Conflict(new { error = result.Message }),
            _ => BadRequest(new { error = result.Message })
        };
    }

    [HttpGet("{code}")]
    [ProducesResponseType<BookingConfirmation>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookingConfirmation> GetByCode(string code)
    {
        var booking = hotelBookingService.GetBookingByCode(code);
        if (booking is null)
        {
            return NotFound(new { error = "Booking was not found." });
        }

        return Ok(booking);
    }
}
