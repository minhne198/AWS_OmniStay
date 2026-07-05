using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Mvc;

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
        [FromQuery] string city,
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int guests)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest(new { error = "City is required." });
        }

        if (checkOut <= checkIn)
        {
            return BadRequest(new { error = "Check-out date must be after check-in date." });
        }

        if (guests <= 0)
        {
            return BadRequest(new { error = "Guest count must be greater than zero." });
        }

        return Ok(hotelBookingService.SearchAvailableRooms(city, checkIn, checkOut, guests));
    }
}
