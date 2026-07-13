namespace HotelBooking.Api.Models;

public sealed class Booking
{
    public int Id { get; set; }

    public string BookingCode { get; set; } = string.Empty;

    public int RoomTypeId { get; set; }

    public RoomType? RoomType { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public DateOnly CheckIn { get; set; }

    public DateOnly CheckOut { get; set; }

    public int Guests { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public DateTimeOffset? PaidAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public HotelReview? Review { get; set; }

    public List<BalanceTransaction> BalanceTransactions { get; set; } = [];

    public List<PaymentTransaction> PaymentTransactions { get; set; } = [];
}
