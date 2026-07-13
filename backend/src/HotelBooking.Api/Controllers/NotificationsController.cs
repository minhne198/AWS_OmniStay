using System.Security.Claims;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController(HotelBookingDbContext dbContext) : ControllerBase
{
    [HttpGet("my")]
    [ProducesResponseType<IReadOnlyList<NotificationSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<NotificationSummary>> GetMyNotifications()
    {
        var userId = CurrentUserId();
        return Ok(dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(100)
            .ToArray()
            .Select(ToSummary)
            .ToArray());
    }

    [HttpPut("{notificationId:int}/read")]
    [HttpPost("{notificationId:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult MarkRead(int notificationId)
    {
        var userId = CurrentUserId();
        var notification = dbContext.Notifications
            .SingleOrDefault(item => item.Id == notificationId && item.UserId == userId);
        if (notification is null)
        {
            return NotFound(new { error = "Notification was not found." });
        }

        notification.IsRead = true;
        dbContext.SaveChanges();
        return NoContent();
    }

    [HttpPut("read-all")]
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult MarkAllRead()
    {
        var userId = CurrentUserId();
        var notifications = dbContext.Notifications
            .Where(item => item.UserId == userId && !item.IsRead)
            .ToArray();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        dbContext.SaveChanges();
        return NoContent();
    }

    private static NotificationSummary ToSummary(HotelBooking.Api.Models.Notification notification)
    {
        return new NotificationSummary(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.LinkUrl,
            notification.IsRead,
            notification.CreatedAt);
    }

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
