using System.Security.Claims;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController(HotelBookingDbContext dbContext, PayOsPaymentClient payOsClient) : ControllerBase
{
    [HttpGet("balance-transactions/my")]
    [ProducesResponseType<IReadOnlyList<BalanceTransactionSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<BalanceTransactionSummary>> GetMyBalanceTransactions()
    {
        var userId = CurrentUserId();
        return Ok(dbContext.BalanceTransactions
            .AsNoTracking()
            .Include(transaction => transaction.User)
            .Include(transaction => transaction.Booking)
            .Where(transaction => transaction.UserId == userId)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Take(100)
            .ToArray()
            .Select(ToSummary)
            .ToArray());
    }

    [HttpPost("balance/top-up/payos")]
    [ProducesResponseType<PayOsTopUpLinkResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PayOsTopUpLinkResponse>> CreatePayOsTopUp(
        CreatePayOsTopUpRequest request,
        CancellationToken cancellationToken)
    {
        if (!payOsClient.Options.IsConfigured)
        {
            return BadRequest(new { error = "PayOS chua duoc cau hinh tren server." });
        }

        if (request.Amount <= 0 || decimal.Truncate(request.Amount) != request.Amount)
        {
            return BadRequest(new { error = "So tien nap phai la so VND nguyen va lon hon 0." });
        }

        var user = dbContext.Users.SingleOrDefault(item => item.Id == CurrentUserId());
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        var now = DateTimeOffset.UtcNow;
        var transaction = new PaymentTransaction
        {
            UserId = user.Id,
            Provider = PaymentProviders.PayOs,
            Purpose = PaymentTransactionPurposes.BalanceTopUp,
            OrderCode = CreatePayOsOrderCode(),
            Amount = request.Amount,
            Currency = "VND",
            Status = PaymentTransactionStatuses.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
        transaction.Description = CreatePayOsDescription(transaction.OrderCode);

        dbContext.PaymentTransactions.Add(transaction);
        dbContext.SaveChanges();

        var returnUrl = SafeAbsoluteHttpUrl(request.ReturnUrl) ?? DefaultProfileUrl("topup-return");
        var cancelUrl = SafeAbsoluteHttpUrl(request.CancelUrl) ?? DefaultProfileUrl("topup-cancel");

        try
        {
            var payOsResult = await payOsClient.CreatePaymentLinkAsync(
                transaction,
                returnUrl,
                cancelUrl,
                cancellationToken);

            transaction.PaymentLinkId = payOsResult.PaymentLinkId;
            transaction.CheckoutUrl = payOsResult.CheckoutUrl;
            transaction.QrCode = payOsResult.QrCode;
            transaction.UpdatedAt = DateTimeOffset.UtcNow;
            dbContext.SaveChanges();
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException)
        {
            transaction.Status = PaymentTransactionStatuses.Failed;
            transaction.FailureReason = ex.Message;
            transaction.UpdatedAt = DateTimeOffset.UtcNow;
            dbContext.SaveChanges();
            return BadRequest(new { error = ex.Message });
        }

        return Ok(new PayOsTopUpLinkResponse(
            transaction.Provider,
            transaction.OrderCode,
            transaction.Amount,
            transaction.Currency,
            transaction.Status,
            transaction.CheckoutUrl,
            transaction.QrCode));
    }

    [HttpGet("withdrawals/my")]
    [ProducesResponseType<IReadOnlyList<WithdrawalRequestSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<WithdrawalRequestSummary>> GetMyWithdrawals()
    {
        var userId = CurrentUserId();
        return Ok(dbContext.WithdrawalRequests
            .AsNoTracking()
            .Include(request => request.User)
            .Where(request => request.UserId == userId)
            .OrderByDescending(request => request.RequestedAt)
            .Take(100)
            .ToArray()
            .Select(ToWithdrawalSummary)
            .ToArray());
    }

    [HttpPost("withdrawals")]
    [ProducesResponseType<WithdrawalRequestSummary>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<WithdrawalRequestSummary> CreateWithdrawal(CreateWithdrawalRequest request)
    {
        var user = dbContext.Users.SingleOrDefault(item => item.Id == CurrentUserId());
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        if (string.IsNullOrWhiteSpace(user.BankName)
            || string.IsNullOrWhiteSpace(user.BankAccountNumber)
            || string.IsNullOrWhiteSpace(user.BankAccountHolder))
        {
            return BadRequest(new { error = "Vui long cap nhat tai khoan ngan hang truoc khi rut tien." });
        }

        var pendingAmount = dbContext.WithdrawalRequests
            .Where(item => item.UserId == user.Id && item.Status == WithdrawalRequestStatuses.Pending)
            .Select(item => item.Amount)
            .ToArray()
            .Sum();
        if (request.Amount <= 0 || request.Amount + pendingAmount > user.Balance)
        {
            return BadRequest(new { error = "So du kha dung khong du cho lenh rut tien nay." });
        }

        var withdrawal = new WithdrawalRequest
        {
            UserId = user.Id,
            Amount = request.Amount,
            Status = WithdrawalRequestStatuses.Pending,
            BankName = user.BankName,
            BankAccountNumber = user.BankAccountNumber,
            BankAccountHolder = user.BankAccountHolder,
            Note = request.Note?.Trim() ?? string.Empty,
            AdminNote = string.Empty,
            RequestedAt = DateTimeOffset.UtcNow
        };

        dbContext.WithdrawalRequests.Add(withdrawal);
        AddNotificationForAdmins(
            "WithdrawalRequested",
            "Co lenh rut tien moi",
            $"{user.Email} yeu cau rut {request.Amount:N0} VND.",
            "/public/admin.html");
        dbContext.SaveChanges();
        withdrawal.User = user;

        return Created($"/api/account/withdrawals/{withdrawal.Id}", ToWithdrawalSummary(withdrawal));
    }

    [HttpGet("owner-profile")]
    [ProducesResponseType<OwnerProfileSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OwnerProfileSummary> GetOwnerProfile()
    {
        var ownerId = CurrentUserId();
        var owner = dbContext.Users
            .AsNoTracking()
            .SingleOrDefault(user => user.Id == ownerId);

        if (owner is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        var hotels = dbContext.Hotels
            .AsNoTracking()
            .Include(hotel => hotel.RoomTypes)
            .Where(hotel => hotel.OwnerUserId == ownerId)
            .OrderBy(hotel => hotel.Name)
            .ToArray()
            .Select(hotel =>
            {
                var hotelBookings = dbContext.Bookings
                    .AsNoTracking()
                    .Include(booking => booking.RoomType)
                    .Where(booking => booking.RoomType != null
                        && booking.RoomType.HotelId == hotel.Id)
                    .ToArray();

                var paidBookings = hotelBookings
                    .Where(booking => booking.PaymentStatus == PaymentStatuses.Paid
                        && booking.Status != BookingStatuses.Cancelled)
                    .ToArray();

                var reviews = dbContext.HotelReviews
                    .AsNoTracking()
                    .Where(review => review.HotelId == hotel.Id)
                    .ToArray();

                return new OwnerHotelProfileSummary(
                    hotel.Id,
                    hotel.Name,
                    hotel.City,
                    hotel.MainImageUrl,
                    hotel.RoomTypes.Count,
                    paidBookings.Sum(booking => booking.TotalPrice),
                    hotelBookings.Length,
                    reviews.Length == 0 ? 0 : Math.Round(reviews.Average(review => review.Rating), 1),
                    reviews.Length);
            })
            .ToArray();

        return Ok(new OwnerProfileSummary(
            owner.Id,
            owner.FullName,
            owner.Email,
            owner.AvatarUrl,
            hotels.Length > 0 ? "Verified" : "Pending",
            hotels.Sum(hotel => hotel.TotalRevenue),
            hotels.Length,
            hotels));
    }

    private static BalanceTransactionSummary ToSummary(BalanceTransaction transaction)
    {
        return new BalanceTransactionSummary(
            transaction.Id,
            transaction.UserId,
            transaction.User?.Email ?? string.Empty,
            transaction.User?.FullName ?? string.Empty,
            transaction.Booking?.BookingCode,
            transaction.Amount,
            transaction.BalanceAfter,
            transaction.Type,
            transaction.Description,
            transaction.CreatedAt);
    }

    private static WithdrawalRequestSummary ToWithdrawalSummary(WithdrawalRequest request)
    {
        return new WithdrawalRequestSummary(
            request.Id,
            request.UserId,
            request.User?.Email ?? string.Empty,
            request.User?.FullName ?? string.Empty,
            request.Amount,
            request.Status,
            request.BankName,
            request.BankAccountNumber,
            request.BankAccountHolder,
            request.Note,
            request.AdminNote,
            request.RequestedAt,
            request.CompletedAt);
    }

    private long CreatePayOsOrderCode()
    {
        var candidate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        while (dbContext.PaymentTransactions.Any(transaction => transaction.OrderCode == candidate))
        {
            candidate++;
        }

        return candidate;
    }

    private static string CreatePayOsDescription(long orderCode)
    {
        return $"OMNITOPUP{Math.Abs(orderCode % 10000):0000}";
    }

    private string DefaultProfileUrl(string walletState)
    {
        var origin = $"{Request.Scheme}://{Request.Host}";
        return $"{origin}/public/profile.html?wallet={Uri.EscapeDataString(walletState)}";
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

    private void AddNotificationForAdmins(string type, string title, string message, string linkUrl)
    {
        var adminIds = dbContext.Users
            .Where(user => user.Role == UserRoles.Admin)
            .Select(user => user.Id)
            .ToArray();

        foreach (var adminId in adminIds)
        {
            dbContext.Notifications.Add(new Notification
            {
                UserId = adminId,
                Type = type,
                Title = title,
                Message = message,
                LinkUrl = linkUrl,
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
    }

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
