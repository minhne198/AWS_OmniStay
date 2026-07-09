using HotelBooking.Api.Contracts;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = UserRoles.Admin)]
[Route("api/admin")]
public sealed class AdminController(HotelBookingDbContext dbContext, PasswordService passwordService) : ControllerBase
{
    [HttpGet("users")]
    [ProducesResponseType<IReadOnlyList<AdminUserSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminUserSummary>> GetUsers()
    {
        return Ok(dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToArray()
            .Select(user => ToSummary(user))
            .ToArray());
    }

    [HttpPost("users")]
    [ProducesResponseType<AdminUserSummary>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminUserSummary>> CreateUser(UpsertUserRequest request)
    {
        if (dbContext.Users.Any(u => u.Email == request.Email))
        {
            return BadRequest(new { error = "Email already exists." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Role = request.Role,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = passwordService.Hash(request.Password);
        }
        else
        {
            return BadRequest(new { error = "Password is required for new users." });
        }

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, ToSummary(user));
    }

    [HttpPut("users/{userId:int}")]
    [ProducesResponseType<AdminUserSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserSummary>> UpdateUser(int userId, UpsertUserRequest request)
    {
        var user = dbContext.Users.SingleOrDefault(u => u.Id == userId);
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        if (dbContext.Users.Any(u => u.Email == request.Email && u.Id != userId))
        {
            return BadRequest(new { error = "Email already exists." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.Role = request.Role;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = passwordService.Hash(request.Password);
        }

        await dbContext.SaveChangesAsync();
        return Ok(ToSummary(user));
    }

    [HttpDelete("users/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = dbContext.Users.SingleOrDefault(u => u.Id == userId);
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static AdminUserSummary ToSummary(User user)
    {
        return new AdminUserSummary(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt);
    }

    [HttpGet("bookings")]
    [ProducesResponseType<IReadOnlyList<AdminBookingSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminBookingSummary>> GetBookings()
    {
        var bookings = dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.User)
            .Include(booking => booking.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .OrderByDescending(booking => booking.CreatedAt)
            .ToArray()
            .Where(booking => booking.RoomType?.Hotel is not null)
            .Select(booking => new AdminBookingSummary(
                booking.BookingCode,
                booking.GuestName,
                booking.GuestEmail,
                booking.User?.Email,
                booking.RoomType!.Hotel!.Name,
                booking.RoomType.Name,
                booking.CheckIn,
                booking.CheckOut,
                booking.Guests,
                booking.TotalPrice,
                booking.Status,
                booking.PaymentStatus,
                booking.CreatedAt))
            .ToArray();

        return Ok(bookings);
    }

    [HttpGet("hotels")]
    [ProducesResponseType<IReadOnlyList<AdminHotelSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminHotelSummary>> GetHotels()
    {
        return Ok(dbContext.Hotels
            .AsNoTracking()
            .Include(hotel => hotel.RoomTypes)
            .OrderBy(hotel => hotel.City)
            .ThenBy(hotel => hotel.Name)
            .ToArray()
            .Select(hotel => ToSummary(hotel))
            .ToArray());
    }

    [HttpPost("hotels")]
    [ProducesResponseType<AdminHotelSummary>(StatusCodes.Status201Created)]
    public ActionResult<AdminHotelSummary> CreateHotel(UpsertHotelRequest request)
    {
        var hotel = new Hotel();
        Apply(request, hotel);

        dbContext.Hotels.Add(hotel);
        dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetHotels), new { id = hotel.Id }, ToSummary(hotel));
    }

    [HttpPut("hotels/{hotelId:int}")]
    [ProducesResponseType<AdminHotelSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AdminHotelSummary> UpdateHotel(int hotelId, UpsertHotelRequest request)
    {
        var hotel = dbContext.Hotels
            .Include(item => item.RoomTypes)
            .SingleOrDefault(item => item.Id == hotelId);

        if (hotel is null)
        {
            return NotFound(new { error = "Hotel was not found." });
        }

        Apply(request, hotel);
        dbContext.SaveChanges();

        return Ok(ToSummary(hotel));
    }

    [HttpGet("room-types")]
    [ProducesResponseType<IReadOnlyList<RoomTypeDetails>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<RoomTypeDetails>> GetRoomTypes([FromQuery] int? hotelId)
    {
        var query = dbContext.RoomTypes.AsNoTracking();
        if (hotelId is not null)
        {
            query = query.Where(roomType => roomType.HotelId == hotelId);
        }

        return Ok(query
            .OrderBy(roomType => roomType.HotelId)
            .ThenBy(roomType => roomType.PricePerNight)
            .ToArray()
            .Select(roomType => ToDetails(roomType))
            .ToArray());
    }

    [HttpPost("room-types")]
    [ProducesResponseType<RoomTypeDetails>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<RoomTypeDetails> CreateRoomType(UpsertRoomTypeRequest request)
    {
        if (!dbContext.Hotels.Any(hotel => hotel.Id == request.HotelId))
        {
            return NotFound(new { error = "Hotel was not found." });
        }

        var roomType = new RoomType();
        Apply(request, roomType);

        dbContext.RoomTypes.Add(roomType);
        dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetRoomTypes), new { hotelId = roomType.HotelId }, ToDetails(roomType));
    }

    [HttpPut("room-types/{roomTypeId:int}")]
    [ProducesResponseType<RoomTypeDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<RoomTypeDetails> UpdateRoomType(int roomTypeId, UpsertRoomTypeRequest request)
    {
        if (!dbContext.Hotels.Any(hotel => hotel.Id == request.HotelId))
        {
            return NotFound(new { error = "Hotel was not found." });
        }

        var roomType = dbContext.RoomTypes.SingleOrDefault(item => item.Id == roomTypeId);
        if (roomType is null)
        {
            return NotFound(new { error = "Room type was not found." });
        }

        Apply(request, roomType);
        dbContext.SaveChanges();

        return Ok(ToDetails(roomType));
    }

    private static void Apply(UpsertHotelRequest request, Hotel hotel)
    {
        hotel.Name = request.Name.Trim();
        hotel.City = request.City.Trim();
        hotel.Address = request.Address.Trim();
        hotel.Description = request.Description.Trim();
        hotel.StarRating = request.StarRating;
        hotel.MainImageUrl = request.MainImageUrl.Trim();
    }

    private static void Apply(UpsertRoomTypeRequest request, RoomType roomType)
    {
        roomType.HotelId = request.HotelId;
        roomType.Name = request.Name.Trim();
        roomType.Description = request.Description.Trim();
        roomType.MaxGuests = request.MaxGuests;
        roomType.PricePerNight = request.PricePerNight;
        roomType.TotalRooms = request.TotalRooms;
        roomType.ImageUrl = request.ImageUrl.Trim();
    }

    private static AdminHotelSummary ToSummary(Hotel hotel)
    {
        return new AdminHotelSummary(
            hotel.Id,
            hotel.Name,
            hotel.City,
            hotel.Address,
            hotel.Description,
            hotel.StarRating,
            hotel.MainImageUrl,
            hotel.RoomTypes.Count);
    }

    private static RoomTypeDetails ToDetails(RoomType roomType)
    {
        return new RoomTypeDetails(
            roomType.Id,
            roomType.HotelId,
            roomType.Name,
            roomType.Description,
            roomType.MaxGuests,
            roomType.PricePerNight,
            roomType.TotalRooms,
            roomType.ImageUrl);
    }
}
