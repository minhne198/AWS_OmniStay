namespace HotelBooking.Api.Models;

public sealed class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = UserRoles.Customer;

    public string AvatarUrl { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public string BankAccountNumber { get; set; } = string.Empty;

    public string BankAccountHolder { get; set; } = string.Empty;

    public decimal Balance { get; set; } = 100_000_000m;

    public DateTimeOffset CreatedAt { get; set; }

    public List<Booking> Bookings { get; set; } = [];

    public List<Hotel> OwnedHotels { get; set; } = [];

    public List<HotelReview> HotelReviews { get; set; } = [];

    public List<Notification> Notifications { get; set; } = [];

    public List<BalanceTransaction> BalanceTransactions { get; set; } = [];

    public List<PaymentTransaction> PaymentTransactions { get; set; } = [];

    public List<WithdrawalRequest> WithdrawalRequests { get; set; } = [];
}
