namespace HotelBooking.Api.Models;

public static class BookingStatuses
{
    public const string PendingPayment = "PendingPayment";

    public const string Confirmed = "Confirmed";

    public const string Cancelled = "Cancelled";

    public static bool HoldsInventory(string status)
    {
        return status is PendingPayment or Confirmed;
    }
}
