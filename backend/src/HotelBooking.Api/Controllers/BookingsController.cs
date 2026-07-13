using HotelBooking.Api.Contracts;
using HotelBooking.Api.Models;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        var result = hotelBookingService.CreateBooking(request, OptionalUserId());
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

    [Authorize]
    [HttpGet("my")]
    [ProducesResponseType<IReadOnlyList<BookingConfirmation>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<BookingConfirmation>> GetMyBookings()
    {
        return Ok(hotelBookingService.GetBookingsForUser(CurrentUserId()));
    }

    [Authorize]
    [HttpPost("{code}/pay")]
    [ProducesResponseType<BookingConfirmation>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookingConfirmation> Pay(string code, MockPaymentRequest request)
    {
        _ = request.PaymentMethod;

        BookingConfirmation? booking;
        try
        {
            booking = hotelBookingService.ConfirmMockPayment(
                code,
                CurrentUserId(),
                User.IsInRole(UserRoles.Admin));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        if (booking is null)
        {
            return NotFound(new { error = "Booking was not found." });
        }

        return Ok(booking);
    }

    [Authorize]
    [HttpDelete("{code}")]
    [HttpPost("{code}/cancel")]
    [ProducesResponseType<BookingConfirmation>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookingConfirmation> Cancel(string code)
    {
        BookingConfirmation? booking;
        try
        {
            booking = hotelBookingService.CancelBooking(
                code,
                CurrentUserId(),
                User.IsInRole(UserRoles.Admin));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        if (booking is null)
        {
            return NotFound(new { error = "Booking was not found." });
        }

        return Ok(booking);
    }

    private int? OptionalUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
