using HotelBooking.Api.Contracts;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin")]
public sealed class AdminController(HotelBookingDbContext dbContext, PasswordService passwordService) : ControllerBase
{
    [Authorize(Roles = UserRoles.Admin)]
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

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost("users")]
    [ProducesResponseType<AdminUserSummary>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminUserSummary>> CreateUser(UpsertUserRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var role = NormalizeRole(request.Role);
        if (role is null)
        {
            return BadRequest(new { error = "Role must be Admin, HotelOwner, or Customer." });
        }

        if (dbContext.Users.Any(u => u.Email == email))
        {
            return BadRequest(new { error = "Email already exists." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = role,
            AvatarUrl = request.AvatarUrl?.Trim() ?? string.Empty,
            Balance = request.Balance ?? 100_000_000m,
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

        AddBalanceTransaction(
            user,
            null,
            user.Balance,
            "AdminBalanceSet",
            "Admin tạo tài khoản với số dư ban đầu");
        AddAdminActivity(
            "AdminBalanceSet",
            "Admin tạo số dư ban đầu",
            $"{user.Email} có số dư ban đầu {user.Balance:N0} VND.",
            "/public/admin.html");
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, ToSummary(user));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("users/{userId:int}")]
    [HttpPost("users/{userId:int}")]
    [ProducesResponseType<AdminUserSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserSummary>> UpdateUser(int userId, UpsertUserRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var role = NormalizeRole(request.Role);
        if (role is null)
        {
            return BadRequest(new { error = "Role must be Admin, HotelOwner, or Customer." });
        }

        var user = dbContext.Users.SingleOrDefault(u => u.Id == userId);
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        if (dbContext.Users.Any(u => u.Email == email && u.Id != userId))
        {
            return BadRequest(new { error = "Email already exists." });
        }

        if (user.Id == CurrentUserId() && role != UserRoles.Admin)
        {
            return BadRequest(new { error = "You cannot remove the Admin role from the account you are currently using." });
        }

        if (user.Role == UserRoles.Admin
            && role != UserRoles.Admin
            && dbContext.Users.Count(item => item.Role == UserRoles.Admin) <= 1)
        {
            return BadRequest(new { error = "At least one Admin account is required." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.Role = role;
        user.AvatarUrl = request.AvatarUrl?.Trim() ?? string.Empty;
        var previousBalance = user.Balance;
        user.Balance = request.Balance ?? user.Balance;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = passwordService.Hash(request.Password);
        }

        if (user.Balance != previousBalance)
        {
            AddBalanceTransaction(
                user,
                null,
                user.Balance - previousBalance,
                "AdminBalanceAdjustment",
                "Admin chỉnh sửa số dư");
            AddAdminActivity(
                "AdminBalanceAdjustment",
                "Admin chỉnh sửa số dư",
                $"{user.Email} được điều chỉnh số dư từ {previousBalance:N0} thành {user.Balance:N0} VND.",
                "/public/admin.html");
        }

        await dbContext.SaveChangesAsync();
        return Ok(ToSummary(user));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost("users/{userId:int}/balance/top-up")]
    [ProducesResponseType<AdminUserSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AdminUserSummary> TopUpBalance(int userId, BalanceTopUpRequest request)
    {
        var user = dbContext.Users.SingleOrDefault(u => u.Id == userId);
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        user.Balance += request.Amount;
        AddBalanceTransaction(
            user,
            null,
            request.Amount,
            "TestTopUp",
            request.Description?.Trim() is { Length: > 0 } description
                ? description
                : "Nạp tiền test");
        AddAdminActivity(
            "TestTopUp",
            "Nạp tiền test",
            $"{user.Email} được nạp thêm {request.Amount:N0} VND.",
            "/public/admin.html");
        dbContext.SaveChanges();

        return Ok(ToSummary(user));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("users/{userId:int}")]
    [HttpPost("users/{userId:int}/delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = dbContext.Users.SingleOrDefault(u => u.Id == userId);
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        if (user.Id == CurrentUserId())
        {
            return BadRequest(new { error = "You cannot delete the account you are currently using." });
        }

        if (user.Role == UserRoles.Admin && dbContext.Users.Count(item => item.Role == UserRoles.Admin) <= 1)
        {
            return BadRequest(new { error = "At least one Admin account is required." });
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
            user.AvatarUrl,
            user.Balance,
            user.CreatedAt);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeRole(string role)
    {
        if (role.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return UserRoles.Admin;
        }

        if (role.Equals(UserRoles.HotelOwner, StringComparison.OrdinalIgnoreCase))
        {
            return UserRoles.HotelOwner;
        }

        if (role.Equals(UserRoles.Customer, StringComparison.OrdinalIgnoreCase))
        {
            return UserRoles.Customer;
        }

        return null;
    }

    [Authorize]
    [HttpGet("bookings")]
    [ProducesResponseType<IReadOnlyList<AdminBookingSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminBookingSummary>> GetBookings()
    {
        var query = dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.User)
            .Include(booking => booking.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .Where(booking => booking.RoomType != null && booking.RoomType.Hotel != null);

        if (!IsAdmin())
        {
            var currentUserId = CurrentUserId();
            query = query.Where(booking => booking.RoomType!.Hotel!.OwnerUserId == currentUserId);
        }

        var bookings = query
            .OrderByDescending(booking => booking.CreatedAt)
            .ToArray()
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

    [Authorize]
    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardSummary>(StatusCodes.Status200OK)]
    public ActionResult<DashboardSummary> GetDashboard()
    {
        var bookings = BookingsForCurrentScope()
            .AsNoTracking()
            .Include(booking => booking.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .Where(booking => booking.RoomType != null && booking.RoomType.Hotel != null)
            .ToArray();

        var paidBookings = bookings
            .Where(booking => booking.PaymentStatus == PaymentStatuses.Paid
                && booking.Status != BookingStatuses.Cancelled)
            .ToArray();

        var activeBookings = bookings
            .Where(booking => BookingStatuses.HoldsInventory(booking.Status))
            .ToArray();

        var roomTypes = RoomTypesForCurrentScope()
            .AsNoTracking()
            .ToArray();

        var totalRooms = roomTypes.Sum(room => room.TotalRooms);
        var bookedRooms = activeBookings.Length;
        var occupancyRate = totalRooms <= 0
            ? 0
            : Math.Round(bookedRooms * 100m / totalRooms, 2);

        var mostBookedRoom = bookings
            .GroupBy(booking => booking.RoomType!.Name)
            .Select(group => new { Name = group.Key, Count = group.Count() })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Name)
            .FirstOrDefault();

        var revenueByDay = paidBookings
            .GroupBy(booking => (booking.PaidAt ?? booking.CreatedAt).UtcDateTime.Date)
            .Select(group => new RevenuePoint(
                group.Key.ToString("yyyy-MM-dd"),
                group.Sum(booking => booking.TotalPrice),
                group.Count()))
            .OrderByDescending(point => point.Label)
            .Take(30)
            .OrderBy(point => point.Label)
            .ToArray();

        var revenueByMonth = paidBookings
            .GroupBy(booking => (booking.PaidAt ?? booking.CreatedAt).UtcDateTime.ToString("yyyy-MM"))
            .Select(group => new RevenuePoint(
                group.Key,
                group.Sum(booking => booking.TotalPrice),
                group.Count()))
            .OrderByDescending(point => point.Label)
            .Take(12)
            .OrderBy(point => point.Label)
            .ToArray();

        return Ok(new DashboardSummary(
            paidBookings.Sum(booking => booking.TotalPrice),
            bookings.Length,
            mostBookedRoom?.Name ?? string.Empty,
            mostBookedRoom?.Count ?? 0,
            totalRooms,
            bookedRooms,
            occupancyRate,
            revenueByDay,
            revenueByMonth));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("balance-transactions")]
    [ProducesResponseType<IReadOnlyList<BalanceTransactionSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<BalanceTransactionSummary>> GetBalanceTransactions()
    {
        return Ok(dbContext.BalanceTransactions
            .AsNoTracking()
            .Include(transaction => transaction.User)
            .Include(transaction => transaction.Booking)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Take(200)
            .ToArray()
            .Select(ToSummary)
            .ToArray());
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("withdrawals")]
    [ProducesResponseType<IReadOnlyList<WithdrawalRequestSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<WithdrawalRequestSummary>> GetWithdrawals()
    {
        return Ok(dbContext.WithdrawalRequests
            .AsNoTracking()
            .Include(request => request.User)
            .OrderBy(request => request.Status == WithdrawalRequestStatuses.Pending ? 0 : 1)
            .ThenByDescending(request => request.RequestedAt)
            .Take(200)
            .ToArray()
            .Select(ToWithdrawalSummary)
            .ToArray());
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost("withdrawals/{withdrawalRequestId:int}/complete")]
    [ProducesResponseType<WithdrawalRequestSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<WithdrawalRequestSummary> CompleteWithdrawal(
        int withdrawalRequestId,
        CompleteWithdrawalRequest request)
    {
        var withdrawal = dbContext.WithdrawalRequests
            .Include(item => item.User)
            .SingleOrDefault(item => item.Id == withdrawalRequestId);
        if (withdrawal?.User is null)
        {
            return NotFound(new { error = "Withdrawal request was not found." });
        }

        if (withdrawal.Status != WithdrawalRequestStatuses.Pending)
        {
            return BadRequest(new { error = "Withdrawal request is not pending." });
        }

        if (withdrawal.User.Balance < withdrawal.Amount)
        {
            return BadRequest(new { error = "User balance is not enough to complete this withdrawal." });
        }

        withdrawal.User.Balance -= withdrawal.Amount;
        withdrawal.Status = WithdrawalRequestStatuses.Completed;
        withdrawal.AdminNote = request.AdminNote?.Trim() ?? string.Empty;
        withdrawal.CompletedAt = DateTimeOffset.UtcNow;
        withdrawal.CompletedByAdminId = CurrentUserId();

        AddBalanceTransaction(
            withdrawal.User,
            null,
            -withdrawal.Amount,
            "WithdrawalCompleted",
            $"Rut tien ve {withdrawal.BankName} - {withdrawal.BankAccountNumber}");
        AddUserNotification(
            withdrawal.UserId,
            "WithdrawalCompleted",
            "Lenh rut tien da hoan tat",
            $"Admin da xac nhan lenh rut {withdrawal.Amount:N0} VND.",
            "/public/profile.html");
        AddAdminActivity(
            "WithdrawalCompleted",
            "Da xac nhan rut tien",
            $"{withdrawal.User.Email} da duoc xac nhan rut {withdrawal.Amount:N0} VND.",
            "/public/admin.html");

        dbContext.SaveChanges();
        return Ok(ToWithdrawalSummary(withdrawal));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost("withdrawals/{withdrawalRequestId:int}/reject")]
    [ProducesResponseType<WithdrawalRequestSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<WithdrawalRequestSummary> RejectWithdrawal(
        int withdrawalRequestId,
        CompleteWithdrawalRequest request)
    {
        var withdrawal = dbContext.WithdrawalRequests
            .Include(item => item.User)
            .SingleOrDefault(item => item.Id == withdrawalRequestId);
        if (withdrawal?.User is null)
        {
            return NotFound(new { error = "Withdrawal request was not found." });
        }

        if (withdrawal.Status != WithdrawalRequestStatuses.Pending)
        {
            return BadRequest(new { error = "Withdrawal request is not pending." });
        }

        withdrawal.Status = WithdrawalRequestStatuses.Rejected;
        withdrawal.AdminNote = request.AdminNote?.Trim() ?? string.Empty;
        withdrawal.CompletedAt = DateTimeOffset.UtcNow;
        withdrawal.CompletedByAdminId = CurrentUserId();

        AddUserNotification(
            withdrawal.UserId,
            "WithdrawalRejected",
            "Lenh rut tien bi tu choi",
            $"Lenh rut {withdrawal.Amount:N0} VND da bi tu choi.",
            "/public/profile.html");
        AddAdminActivity(
            "WithdrawalRejected",
            "Tu choi rut tien",
            $"{withdrawal.User.Email} bi tu choi lenh rut {withdrawal.Amount:N0} VND.",
            "/public/admin.html");

        dbContext.SaveChanges();
        return Ok(ToWithdrawalSummary(withdrawal));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("activity")]
    [ProducesResponseType<IReadOnlyList<AdminActivitySummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminActivitySummary>> GetActivity()
    {
        return Ok(dbContext.Notifications
            .AsNoTracking()
            .Include(notification => notification.User)
            .Where(notification => notification.User != null && notification.User.Role == UserRoles.Admin)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(200)
            .ToArray()
            .Select(notification => new AdminActivitySummary(
                notification.Id,
                notification.UserId,
                notification.User?.Email ?? string.Empty,
                notification.Type,
                notification.Title,
                notification.Message,
                notification.LinkUrl,
                notification.CreatedAt))
            .ToArray());
    }

    [Authorize]
    [HttpGet("hotels")]
    [ProducesResponseType<IReadOnlyList<AdminHotelSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminHotelSummary>> GetHotels()
    {
        return Ok(HotelsForCurrentUser()
            .AsNoTracking()
            .Include(hotel => hotel.RoomTypes)
            .OrderBy(hotel => hotel.City)
            .ThenBy(hotel => hotel.Name)
            .ToArray()
            .Select(hotel => ToSummary(hotel))
            .ToArray());
    }

    [Authorize]
    [HttpPost("hotels")]
    [ProducesResponseType<AdminHotelSummary>(StatusCodes.Status201Created)]
    public ActionResult<AdminHotelSummary> CreateHotel(UpsertHotelRequest request)
    {
        var hotel = new Hotel();
        Apply(request, hotel);
        if (!IsAdmin())
        {
            hotel.OwnerUserId = CurrentUserId();
        }

        dbContext.Hotels.Add(hotel);
        dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetHotels), new { id = hotel.Id }, ToSummary(hotel));
    }

    [Authorize]
    [HttpPut("hotels/{hotelId:int}")]
    [HttpPost("hotels/{hotelId:int}")]
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

        if (!CanManageHotel(hotel))
        {
            return BadRequest(new { error = "Current account cannot manage this hotel." });
        }

        Apply(request, hotel);
        dbContext.SaveChanges();

        return Ok(ToSummary(hotel));
    }

    [Authorize]
    [HttpGet("room-types")]
    [ProducesResponseType<IReadOnlyList<RoomTypeDetails>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<RoomTypeDetails>> GetRoomTypes([FromQuery] int? hotelId)
    {
        var query = dbContext.RoomTypes
            .AsNoTracking()
            .Include(roomType => roomType.Hotel)
            .AsQueryable();

        if (!IsAdmin())
        {
            var currentUserId = CurrentUserId();
            query = query.Where(roomType => roomType.Hotel!.OwnerUserId == currentUserId);
        }

        if (hotelId is not null)
        {
            query = query.Where(roomType => roomType.HotelId == hotelId);
        }

        return Ok(query
            .OrderBy(roomType => roomType.HotelId)
            .ThenBy(roomType => roomType.PricePerNight)
            .ToArray()
            .Select(roomType => ToDetails(roomType, ActiveBookedRooms(roomType.Id)))
            .ToArray());
    }

    [Authorize]
    [HttpPost("room-types")]
    [ProducesResponseType<RoomTypeDetails>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<RoomTypeDetails> CreateRoomType(UpsertRoomTypeRequest request)
    {
        var hotel = dbContext.Hotels.SingleOrDefault(hotel => hotel.Id == request.HotelId);
        if (hotel is null)
        {
            return NotFound(new { error = "Hotel was not found." });
        }

        if (!CanManageHotel(hotel))
        {
            return BadRequest(new { error = "Current account cannot manage this hotel." });
        }

        var roomType = new RoomType();
        Apply(request, roomType);

        dbContext.RoomTypes.Add(roomType);
        dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetRoomTypes), new { hotelId = roomType.HotelId }, ToDetails(roomType, 0));
    }

    [Authorize]
    [HttpPut("room-types/{roomTypeId:int}")]
    [HttpPost("room-types/{roomTypeId:int}")]
    [ProducesResponseType<RoomTypeDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<RoomTypeDetails> UpdateRoomType(int roomTypeId, UpsertRoomTypeRequest request)
    {
        var targetHotel = dbContext.Hotels.SingleOrDefault(hotel => hotel.Id == request.HotelId);
        if (targetHotel is null)
        {
            return NotFound(new { error = "Hotel was not found." });
        }

        var roomType = dbContext.RoomTypes
            .Include(item => item.Hotel)
            .SingleOrDefault(item => item.Id == roomTypeId);
        if (roomType is null)
        {
            return NotFound(new { error = "Room type was not found." });
        }

        if (!CanManageHotel(roomType.Hotel) || !CanManageHotel(targetHotel))
        {
            return BadRequest(new { error = "Current account cannot manage this room type." });
        }

        Apply(request, roomType);
        dbContext.SaveChanges();

        return Ok(ToDetails(roomType, ActiveBookedRooms(roomType.Id)));
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
        roomType.IsHidden = request.IsHidden;
    }

    private static AdminHotelSummary ToSummary(Hotel hotel)
    {
        return new AdminHotelSummary(
            hotel.Id,
            hotel.OwnerUserId,
            hotel.Name,
            hotel.City,
            hotel.Address,
            hotel.Description,
            hotel.StarRating,
            hotel.MainImageUrl,
            hotel.RoomTypes.Count);
    }

    private static RoomTypeDetails ToDetails(RoomType roomType, int bookedRooms)
    {
        return new RoomTypeDetails(
            roomType.Id,
            roomType.HotelId,
            roomType.Name,
            roomType.Description,
            roomType.MaxGuests,
            roomType.PricePerNight,
            roomType.TotalRooms,
            roomType.ImageUrl,
            roomType.IsHidden,
            bookedRooms,
            Math.Max(0, roomType.TotalRooms - bookedRooms));
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

    private int ActiveBookedRooms(int roomTypeId)
    {
        return dbContext.Bookings.Count(booking =>
            booking.RoomTypeId == roomTypeId
            && (booking.Status == BookingStatuses.PendingPayment
                || booking.Status == BookingStatuses.Confirmed));
    }

    private IQueryable<Booking> BookingsForCurrentScope()
    {
        var query = dbContext.Bookings.AsQueryable();
        if (!IsAdmin())
        {
            var currentUserId = CurrentUserId();
            query = query.Where(booking =>
                booking.RoomType != null
                && booking.RoomType.Hotel != null
                && booking.RoomType.Hotel.OwnerUserId == currentUserId);
        }

        return query;
    }

    private IQueryable<RoomType> RoomTypesForCurrentScope()
    {
        var query = dbContext.RoomTypes.AsQueryable();
        if (!IsAdmin())
        {
            var currentUserId = CurrentUserId();
            query = query.Where(roomType => roomType.Hotel != null && roomType.Hotel.OwnerUserId == currentUserId);
        }

        return query;
    }

    private IQueryable<Hotel> HotelsForCurrentUser()
    {
        var query = dbContext.Hotels.AsQueryable();
        if (!IsAdmin())
        {
            var currentUserId = CurrentUserId();
            query = query.Where(hotel => hotel.OwnerUserId == currentUserId);
        }

        return query;
    }

    private bool CanManageHotel(Hotel? hotel)
    {
        return hotel is not null;
    }

    private bool IsAdmin()
    {
        return User.IsInRole(UserRoles.Admin);
    }

    private void AddBalanceTransaction(
        User user,
        Booking? booking,
        decimal amount,
        string type,
        string description)
    {
        dbContext.BalanceTransactions.Add(new BalanceTransaction
        {
            UserId = user.Id,
            BookingId = booking?.Id,
            Amount = amount,
            BalanceAfter = user.Balance,
            Type = type,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private void AddAdminActivity(string type, string title, string message, string linkUrl)
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
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
    }

    private void AddUserNotification(int userId, string type, string title, string message, string linkUrl)
    {
        dbContext.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            LinkUrl = linkUrl,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
