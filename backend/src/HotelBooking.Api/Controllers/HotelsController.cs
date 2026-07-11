using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/hotels")]
public sealed class HotelsController(IHotelBookingService hotelBookingService) : ControllerBase
{
    [HttpGet("{hotelId:int}")]
    [ProducesResponseType<HotelDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<HotelDetails> GetById(int hotelId)
    {
        var hotel = hotelBookingService.GetHotelById(hotelId);
        if (hotel is null)
        {
            return NotFound(new { error = "Hotel was not found." });
        }

        return Ok(hotel);
    }

    [HttpGet("{hotelId:int}/rooms")]
    [ProducesResponseType<IReadOnlyList<RoomTypeDetails>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<RoomTypeDetails>> GetRooms(int hotelId)
    {
        if (hotelBookingService.GetHotelById(hotelId) is null)
        {
            return NotFound(new { error = "Hotel was not found." });
        }

        return Ok(hotelBookingService.GetRoomsByHotelId(hotelId));
    }

    [HttpGet("search")]
    [ProducesResponseType<IReadOnlyList<HotelSearchResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<HotelSearchResult>> Search(
        [FromQuery] string? city,
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int guests,
        [FromQuery] string? keyword,
        [FromQuery] int? minRating,
        [FromQuery] string? sortBy)
    {
        if (checkOut <= checkIn)
        {
            return BadRequest(new { error = "Check-out date must be after check-in date." });
        }

        if (guests <= 0)
        {
            return BadRequest(new { error = "Guest count must be greater than zero." });
        }

        return Ok(hotelBookingService.SearchAvailableRooms(
            city ?? string.Empty,
            checkIn,
            checkOut,
            guests,
            keyword,
            minRating,
            sortBy));
    }

    [HttpGet("{hotelId:int}/reviews")]
    [ProducesResponseType<IReadOnlyList<HotelReviewSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<HotelReviewSummary>> GetReviews(int hotelId)
    {
        if (hotelBookingService.GetHotelById(hotelId) is null)
        {
            return NotFound(new { error = "Hotel was not found." });
        }

        return Ok(hotelBookingService.GetHotelReviews(hotelId));
    }

    [Authorize]
    [HttpPost("{hotelId:int}/reviews")]
    [ProducesResponseType<HotelReviewSummary>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<HotelReviewSummary> CreateReview(int hotelId, CreateHotelReviewRequest request)
    {
        try
        {
            var review = hotelBookingService.CreateReview(hotelId, request, CurrentUserId());
            return CreatedAtAction(nameof(GetReviews), new { hotelId }, review);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
