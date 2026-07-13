using System.Security.Claims;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController(HotelBookingDbContext dbContext) : ControllerBase
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

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
