using System.Text.Json;

namespace HotelBooking.Api.Contracts;

public sealed record MockPaymentRequest(string PaymentMethod);

public sealed record CreatePayOsPaymentRequest(
    string? ReturnUrl = null,
    string? CancelUrl = null);

public sealed record CreatePayOsTopUpRequest(
    [System.ComponentModel.DataAnnotations.Range(1, 1_000_000_000_000)] decimal Amount,
    string? ReturnUrl = null,
    string? CancelUrl = null);

public sealed record PayOsPaymentLinkResponse(
    string BookingCode,
    string Provider,
    long OrderCode,
    decimal Amount,
    string Currency,
    string Status,
    string CheckoutUrl,
    string QrCode);

public sealed record PayOsTopUpLinkResponse(
    string Provider,
    long OrderCode,
    decimal Amount,
    string Currency,
    string Status,
    string CheckoutUrl,
    string QrCode);

public sealed record PayOsWebhookResult(
    bool Accepted,
    string Message);

public sealed record PayOsWebhookRequest(
    string? Code,
    string? Desc,
    bool Success,
    JsonElement Data,
    string? Signature);

public sealed record CreateWithdrawalRequest(
    [System.ComponentModel.DataAnnotations.Range(1, 1_000_000_000_000)] decimal Amount,
    [System.ComponentModel.DataAnnotations.MaxLength(1_000)] string? Note = null);

public sealed record WithdrawalRequestSummary(
    int WithdrawalRequestId,
    int UserId,
    string UserEmail,
    string UserFullName,
    decimal Amount,
    string Status,
    string BankName,
    string BankAccountNumber,
    string BankAccountHolder,
    string Note,
    string AdminNote,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt);

public sealed record CompleteWithdrawalRequest(
    [System.ComponentModel.DataAnnotations.MaxLength(1_000)] string? AdminNote = null);
