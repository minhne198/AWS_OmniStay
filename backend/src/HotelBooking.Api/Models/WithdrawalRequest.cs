namespace HotelBooking.Api.Models;

public sealed class WithdrawalRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public string BankAccountNumber { get; set; } = string.Empty;

    public string BankAccountHolder { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    public string AdminNote { get; set; } = string.Empty;

    public DateTimeOffset RequestedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public int? CompletedByAdminId { get; set; }

    public User? CompletedByAdmin { get; set; }
}
