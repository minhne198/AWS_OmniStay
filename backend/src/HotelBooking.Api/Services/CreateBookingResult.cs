using HotelBooking.Api.Contracts;

namespace HotelBooking.Api.Services;

public sealed record CreateBookingResult(
    BookingConfirmation? Confirmation,
    BookingFailureReason? FailureReason,
    string? Message)
{
    public static CreateBookingResult Success(BookingConfirmation confirmation)
    {
        return new CreateBookingResult(confirmation, null, null);
    }

    public static CreateBookingResult Failure(BookingFailureReason reason, string message)
    {
        return new CreateBookingResult(null, reason, message);
    }
}
