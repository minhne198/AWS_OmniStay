using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.Contracts;

public sealed record RegisterRequest(
    [Required, MaxLength(200)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [MaxLength(30)] string? Role = null);

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MaxLength(100)] string Password);

public sealed record AuthResponse(
    string Token,
    UserSummary User);

public sealed record UserSummary(
    int UserId,
    string FullName,
    string Email,
    string Role,
    string AvatarUrl,
    string BankName,
    string BankAccountNumber,
    string BankAccountHolder,
    decimal Balance);

public sealed record UpdateProfileRequest(
    [Required, MaxLength(200)] string FullName,
    [MaxLength(500)] string? AvatarUrl);

public sealed record UpdateBankAccountRequest(
    [MaxLength(100)] string? BankName,
    [MaxLength(50)] string? BankAccountNumber,
    [MaxLength(200)] string? BankAccountHolder);

public sealed record ChangePasswordRequest(
    [Required, MaxLength(100)] string CurrentPassword,
    [Required, MinLength(6), MaxLength(100)] string NewPassword);
