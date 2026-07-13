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
    [HttpGet("payments/payos/{orderCode:long}")]
    [ProducesResponseType<BookingConfirmation>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookingConfirmation> GetByPayOsOrderCode(long orderCode)
    {
        var booking = hotelBookingService.GetBookingByPayOsOrderCode(
            orderCode,
            CurrentUserId(),
            User.IsInRole(UserRoles.Admin));
        if (booking is null)
        {
            return NotFound(new { error = "Booking was not found." });
        }

        return Ok(booking);
    }

    [Authorize]
    [HttpPost("{code}/payments/payos")]
    [ProducesResponseType<PayOsPaymentLinkResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayOsPaymentLinkResponse>> CreatePayOsPayment(
        string code,
        CreatePayOsPaymentRequest? request,
        CancellationToken cancellationToken)
    {
        var returnUrl = SafeAbsoluteHttpUrl(request?.ReturnUrl)
            ?? DefaultBookingUrl(code, "payos-return");
        var cancelUrl = SafeAbsoluteHttpUrl(request?.CancelUrl)
            ?? DefaultBookingUrl(code, "payos-cancel");

        PayOsPaymentLinkResponse? payment;
        try
        {
            payment = await hotelBookingService.CreatePayOsPaymentAsync(
                code,
                CurrentUserId(),
                User.IsInRole(UserRoles.Admin),
                returnUrl,
                cancelUrl,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        if (payment is null)
        {
            return NotFound(new { error = "Booking was not found." });
        }

        return Ok(payment);
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

    private string DefaultBookingUrl(string code, string paymentState)
    {
        var origin = $"{Request.Scheme}://{Request.Host}";
        return $"{origin}/public/booking-confirmation.html?bookingCode={Uri.EscapeDataString(code)}&payment={Uri.EscapeDataString(paymentState)}";
    }

    private static string? SafeAbsoluteHttpUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps
            ? uri.ToString()
            : null;
    }
}
