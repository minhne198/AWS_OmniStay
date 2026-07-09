using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.Contracts;

public sealed record RegisterRequest(
    [Required, MaxLength(200)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password);

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
    string Role);
