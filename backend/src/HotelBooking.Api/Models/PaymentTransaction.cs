namespace HotelBooking.Api.Models;

public sealed class PaymentTransaction
{
    public int Id { get; set; }

    public int? BookingId { get; set; }

    public Booking? Booking { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public long OrderCode { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public string Status { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string PaymentLinkId { get; set; } = string.Empty;

    public string CheckoutUrl { get; set; } = string.Empty;

    public string QrCode { get; set; } = string.Empty;

    public string ProviderReference { get; set; } = string.Empty;

    public string FailureReason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }
}
