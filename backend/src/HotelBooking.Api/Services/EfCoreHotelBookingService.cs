using System.Globalization;
using System.Text.Json;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public sealed class EfCoreHotelBookingService(HotelBookingDbContext dbContext, PayOsPaymentClient payOsClient) : IHotelBookingService
{
    public HotelDetails? GetHotelById(int hotelId)
    {
        return dbContext.Hotels
            .AsNoTracking()
            .Where(hotel => hotel.Id == hotelId)
            .Select(hotel => new HotelDetails(
                hotel.Id,
                hotel.Name,
                hotel.City,
                hotel.Address,
                hotel.Description,
                hotel.StarRating,
                hotel.MainImageUrl,
                dbContext.HotelReviews
                    .Where(review => review.HotelId == hotel.Id)
                    .Select(review => (double?)review.Rating)
                    .Average() ?? 0,
                dbContext.HotelReviews.Count(review => review.HotelId == hotel.Id)))
            .SingleOrDefault();
    }

    public IReadOnlyList<RoomTypeDetails> GetRoomsByHotelId(int hotelId)
    {
        return dbContext.RoomTypes
            .AsNoTracking()
            .Where(roomType => roomType.HotelId == hotelId)
            .Where(roomType => !roomType.IsHidden)
            .OrderBy(roomType => roomType.PricePerNight)
            .Select(roomType => new
            {
                RoomType = roomType,
                BookedRooms = dbContext.Bookings.Count(booking =>
                    booking.RoomTypeId == roomType.Id
                    && (booking.Status == BookingStatuses.PendingPayment
                        || booking.Status == BookingStatuses.Confirmed))
            })
            .ToArray()
            .Select(item => ToDetails(item.RoomType, item.BookedRooms))
            .ToArray();
    }

    public IReadOnlyList<HotelSearchResult> SearchAvailableRooms(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests,
        string? keyword = null,
        int? minRating = null,
        string? sortBy = null)
    {
        if (checkOut <= checkIn || guests <= 0)
        {
            return [];
        }

        var normalizedCity = city.Trim();
        var normalizedKeyword = keyword?.Trim() ?? string.Empty;

        var query = dbContext.RoomTypes
            .AsNoTracking()
            .Where(roomType => roomType.Hotel != null)
            .Where(roomType => !roomType.IsHidden)
            .Where(roomType => roomType.MaxGuests >= guests);

        if (!string.IsNullOrWhiteSpace(normalizedCity))
        {
            query = query.Where(roomType => roomType.Hotel!.City == normalizedCity);
        }

        if (!string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            query = query.Where(roomType =>
                roomType.Hotel!.Name.Contains(normalizedKeyword)
                || roomType.Name.Contains(normalizedKeyword));
        }

        var results = query
            .Select(roomType => new
            {
                RoomType = roomType,
                Hotel = roomType.Hotel!,
                AverageRating = dbContext.HotelReviews
                    .Where(review => review.HotelId == roomType.HotelId)
                    .Select(review => (double?)review.Rating)
                    .Average() ?? 0,
                ReviewCount = dbContext.HotelReviews.Count(review => review.HotelId == roomType.HotelId),
                BookedRooms = dbContext.Bookings.Count(booking =>
                    booking.RoomTypeId == roomType.Id
                    && (booking.Status == BookingStatuses.PendingPayment
                        || booking.Status == BookingStatuses.Confirmed)
                    && booking.CheckIn < checkOut
                    && booking.CheckOut > checkIn)
            })
            .ToArray();

        var available = results
            .Select(item => new
            {
                item.RoomType,
                item.Hotel,
                AverageRating = Math.Round(item.AverageRating, 1),
                item.ReviewCount,
                AvailableRooms = item.RoomType.TotalRooms - item.BookedRooms
            })
            .Where(item => item.AvailableRooms > 0);

        if (minRating is > 0)
        {
            available = available.Where(item => item.AverageRating >= minRating.Value);
        }

        available = NormalizeSort(sortBy) switch
        {
            "rating" => available
                .OrderByDescending(item => item.AverageRating)
                .ThenByDescending(item => item.ReviewCount)
                .ThenBy(item => item.RoomType.PricePerNight),
            "price_desc" => available
                .OrderByDescending(item => item.RoomType.PricePerNight)
                .ThenByDescending(item => item.AverageRating),
            _ => available
                .OrderBy(item => item.RoomType.PricePerNight)
                .ThenByDescending(item => item.AverageRating)
        };

        return available
            .Select(item => new HotelSearchResult(
                HotelId: item.Hotel.Id,
                RoomTypeId: item.RoomType.Id,
                HotelName: item.Hotel.Name,
                City: item.Hotel.City,
                RoomTypeName: item.RoomType.Name,
                MainImageUrl: item.Hotel.MainImageUrl,
                RoomImageUrl: item.RoomType.ImageUrl,
                MaxGuests: item.RoomType.MaxGuests,
                PricePerNight: item.RoomType.PricePerNight,
                AvailableRooms: item.AvailableRooms,
                AverageRating: item.AverageRating,
                ReviewCount: item.ReviewCount))
            .ToArray();
    }

    public CreateBookingResult CreateBooking(CreateBookingRequest request, int? userId = null)
    {
        if (request.CheckOut <= request.CheckIn)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.InvalidDates,
                "Check-out date must be after check-in date.");
        }

        var roomType = dbContext.RoomTypes
            .Include(room => room.Hotel)
            .ThenInclude(hotel => hotel!.Owner)
            .SingleOrDefault(room => room.Id == request.RoomTypeId);

        if (roomType is null)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.RoomTypeNotFound,
                "Room type was not found.");
        }

        if (roomType.IsHidden)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.RoomTypeNotFound,
                "Room type was not found.");
        }

        if (request.Guests <= 0 || request.Guests > roomType.MaxGuests)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.InvalidGuests,
                "Guest count is invalid for this room type.");
        }

        var bookedRooms = dbContext.Bookings.Count(booking =>
            booking.RoomTypeId == roomType.Id
            && (booking.Status == BookingStatuses.PendingPayment
                || booking.Status == BookingStatuses.Confirmed)
            && booking.CheckIn < request.CheckOut
            && booking.CheckOut > request.CheckIn);

        if (roomType.TotalRooms - bookedRooms <= 0)
        {
            return CreateBookingResult.Failure(
                BookingFailureReason.SoldOut,
                "This room type is sold out for the selected dates.");
        }

        var nights = CalculateNights(request.CheckIn, request.CheckOut);
        var booking = new Booking
        {
            BookingCode = CreateBookingCode(request.CheckIn),
            RoomTypeId = roomType.Id,
            UserId = userId,
            GuestName = request.GuestName.Trim(),
            GuestEmail = request.GuestEmail.Trim(),
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Guests = request.Guests,
            TotalPrice = nights * roomType.PricePerNight,
            Status = BookingStatuses.PendingPayment,
            PaymentStatus = PaymentStatuses.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Bookings.Add(booking);
        AddBookingCreatedNotifications(booking, roomType);
        dbContext.SaveChanges();

        return CreateBookingResult.Success(ToConfirmation(booking, roomType));
    }

    public BookingConfirmation? GetBookingByCode(string bookingCode)
    {
        if (string.IsNullOrWhiteSpace(bookingCode))
        {
            return null;
        }

        var booking = dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .Where(item => item.BookingCode == bookingCode.Trim())
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .FirstOrDefault();

        return booking?.RoomType?.Hotel is null ? null : ToConfirmation(booking, booking.RoomType);
    }

    public BookingConfirmation? GetBookingByPayOsOrderCode(long orderCode, int userId, bool isAdmin = false)
    {
        var query = dbContext.PaymentTransactions
            .AsNoTracking()
            .Include(transaction => transaction.Booking)
            .ThenInclude(booking => booking!.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .Where(transaction => transaction.Provider == PaymentProviders.PayOs)
            .Where(transaction => transaction.OrderCode == orderCode)
            .Select(transaction => transaction.Booking!);

        if (!isAdmin)
        {
            query = query.Where(booking => booking.UserId == userId);
        }

        var booking = query
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .FirstOrDefault();

        return booking?.RoomType?.Hotel is null ? null : ToConfirmation(booking, booking.RoomType);
    }

    public IReadOnlyList<BookingConfirmation> GetBookingsForUser(int userId)
    {
        return dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ToArray()
            .Where(item => item.RoomType is not null)
            .Select(item => ToConfirmation(item, item.RoomType!))
            .ToArray();
    }

    public BookingConfirmation? ConfirmMockPayment(string bookingCode, int userId, bool isAdmin = false)
    {
        var booking = FindMutableBooking(bookingCode, userId, isAdmin);
        if (booking?.RoomType?.Hotel is null)
        {
            return null;
        }

        if (booking.Status == BookingStatuses.Cancelled)
        {
            return null;
        }

        if (booking.PaymentStatus != PaymentStatuses.Paid)
        {
            CompleteBookingPayment(booking, deductPayerBalance: true);
            dbContext.SaveChanges();
        }

        return ToConfirmation(booking, booking.RoomType);
    }

    public async Task<PayOsPaymentLinkResponse?> CreatePayOsPaymentAsync(
        string bookingCode,
        int userId,
        bool isAdmin,
        string returnUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        if (!payOsClient.Options.IsConfigured)
        {
            throw new InvalidOperationException("PayOS chua duoc cau hinh tren server.");
        }

        var booking = FindMutableBooking(bookingCode, userId, isAdmin);
        if (booking?.RoomType?.Hotel is null)
        {
            return null;
        }

        if (booking.Status == BookingStatuses.Cancelled)
        {
            throw new InvalidOperationException("Booking da bi huy, khong the tao thanh toan payOS.");
        }

        if (booking.PaymentStatus == PaymentStatuses.Paid)
        {
            throw new InvalidOperationException("Booking nay da duoc thanh toan.");
        }

        var now = DateTimeOffset.UtcNow;
        var transaction = new PaymentTransaction
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            Provider = PaymentProviders.PayOs,
            Purpose = PaymentTransactionPurposes.BookingPayment,
            OrderCode = CreatePayOsOrderCode(),
            Amount = booking.TotalPrice,
            Currency = "VND",
            Status = PaymentTransactionStatuses.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
        transaction.Description = CreatePayOsDescription(transaction.OrderCode);

        dbContext.PaymentTransactions.Add(transaction);
        dbContext.SaveChanges();

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
            throw;
        }

        return new PayOsPaymentLinkResponse(
            booking.BookingCode,
            transaction.Provider,
            transaction.OrderCode,
            transaction.Amount,
            transaction.Currency,
            transaction.Status,
            transaction.CheckoutUrl,
            transaction.QrCode);
    }

    public Task<PayOsWebhookResult> ProcessPayOsWebhookAsync(
        JsonElement data,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        if (!payOsClient.IsValidWebhookSignature(data, signature))
        {
            return Task.FromResult(new PayOsWebhookResult(false, "Invalid payOS signature."));
        }

        if (!TryGetInt64(data, "orderCode", out var orderCode))
        {
            return Task.FromResult(new PayOsWebhookResult(false, "Missing payOS orderCode."));
        }

        var transaction = dbContext.PaymentTransactions
            .Include(item => item.User)
            .Include(item => item.Booking)
            .ThenInclude(booking => booking!.User)
            .Include(item => item.Booking)
            .ThenInclude(booking => booking!.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .ThenInclude(hotel => hotel!.Owner)
            .SingleOrDefault(item => item.Provider == PaymentProviders.PayOs && item.OrderCode == orderCode);

        if (transaction is null)
        {
            return Task.FromResult(new PayOsWebhookResult(true, "No matching local payOS transaction."));
        }

        if (transaction.Status == PaymentTransactionStatuses.Paid)
        {
            return Task.FromResult(new PayOsWebhookResult(true, "Transaction was already paid."));
        }

        var providerCode = GetString(data, "code");
        if (providerCode != "00")
        {
            transaction.Status = PaymentTransactionStatuses.Failed;
            transaction.FailureReason = GetString(data, "desc") ?? "PayOS payment was not successful.";
            transaction.UpdatedAt = DateTimeOffset.UtcNow;
            dbContext.SaveChanges();
            return Task.FromResult(new PayOsWebhookResult(true, "PayOS transaction marked failed."));
        }

        if (!TryGetDecimal(data, "amount", out var amount) || amount != transaction.Amount)
        {
            transaction.Status = PaymentTransactionStatuses.Failed;
            transaction.FailureReason = "PayOS amount does not match booking total.";
            transaction.UpdatedAt = DateTimeOffset.UtcNow;
            dbContext.SaveChanges();
            return Task.FromResult(new PayOsWebhookResult(false, "PayOS amount does not match booking total."));
        }

        var now = DateTimeOffset.UtcNow;
        transaction.Status = PaymentTransactionStatuses.Paid;
        transaction.ProviderReference = GetString(data, "reference") ?? string.Empty;
        transaction.PaymentLinkId = GetString(data, "paymentLinkId") ?? transaction.PaymentLinkId;
        transaction.PaidAt = now;
        transaction.UpdatedAt = now;

        if (transaction.Purpose == PaymentTransactionPurposes.BalanceTopUp)
        {
            if (transaction.User is null)
            {
                transaction.Status = PaymentTransactionStatuses.Failed;
                transaction.FailureReason = "Top-up transaction has no user.";
                transaction.UpdatedAt = DateTimeOffset.UtcNow;
                dbContext.SaveChanges();
                return Task.FromResult(new PayOsWebhookResult(false, "Top-up transaction has no user."));
            }

            transaction.User.Balance += transaction.Amount;
            AddBalanceTransaction(
                transaction.User,
                null,
                transaction.Amount,
                "PayOsTopUp",
                $"Nap tien payOS order {transaction.OrderCode}");
            AddNotification(
                transaction.User.Id,
                "BalanceTopUpPaid",
                "Nap tien thanh cong",
                $"Tai khoan da duoc cong {transaction.Amount:N0} VND.",
                "/public/profile.html");
            AddNotificationForAdmins(
                "BalanceTopUpPaid",
                "Khach nap tien thanh cong",
                $"{transaction.User.Email} da nap {transaction.Amount:N0} VND qua payOS.",
                "/public/admin.html");
        }
        else if (transaction.Booking?.RoomType?.Hotel is null)
        {
            transaction.Status = PaymentTransactionStatuses.Failed;
            transaction.FailureReason = "Booking payment transaction has no booking.";
            transaction.UpdatedAt = DateTimeOffset.UtcNow;
            dbContext.SaveChanges();
            return Task.FromResult(new PayOsWebhookResult(false, "Booking payment transaction has no booking."));
        }
        else if (transaction.Booking.PaymentStatus != PaymentStatuses.Paid)
        {
            CompleteBookingPayment(transaction.Booking, deductPayerBalance: false);
        }

        dbContext.SaveChanges();
        return Task.FromResult(new PayOsWebhookResult(true, "PayOS transaction accepted."));
    }

    public BookingConfirmation? CancelBooking(string bookingCode, int userId, bool isAdmin = false)
    {
        var booking = FindMutableBooking(bookingCode, userId, isAdmin);
        if (booking?.RoomType?.Hotel is null)
        {
            return null;
        }

        if (booking.Status == BookingStatuses.Cancelled)
        {
            return ToConfirmation(booking, booking.RoomType);
        }

        if (!CanCancelBooking(booking))
        {
            throw new InvalidOperationException("Chỉ có thể hủy phòng trước ngày nhận phòng hơn 24 giờ.");
        }

        if (booking.PaymentStatus == PaymentStatuses.Paid)
        {
            var payer = booking.User;
            if (payer is not null)
            {
                payer.Balance += booking.TotalPrice;
                AddBalanceTransaction(
                    payer,
                    booking,
                    booking.TotalPrice,
                    "BookingRefund",
                    $"Hoàn tiền booking {booking.BookingCode}");
            }

            var owner = booking.RoomType.Hotel.Owner;
            if (owner is not null && owner.Id != payer?.Id)
            {
                owner.Balance -= booking.TotalPrice;
                AddBalanceTransaction(
                    owner,
                    booking,
                    -booking.TotalPrice,
                    "OwnerBookingReversal",
                    $"Trừ lại doanh thu booking hủy {booking.BookingCode}");
            }

            booking.PaymentStatus = PaymentStatuses.Refunded;
        }
        else
        {
            booking.PaymentStatus = PaymentStatuses.Cancelled;
        }

        booking.Status = BookingStatuses.Cancelled;
        AddCancellationNotifications(booking);
        dbContext.SaveChanges();

        return ToConfirmation(booking, booking.RoomType);
    }

    public IReadOnlyList<HotelReviewSummary> GetHotelReviews(int hotelId)
    {
        return dbContext.HotelReviews
            .AsNoTracking()
            .Include(review => review.Booking)
            .Include(review => review.User)
            .Where(review => review.HotelId == hotelId)
            .OrderByDescending(review => review.CreatedAt)
            .ToArray()
            .Select(review => ToReviewSummary(review))
            .ToArray();
    }

    public HotelReviewSummary CreateReview(int hotelId, CreateHotelReviewRequest request, int userId)
    {
        var booking = dbContext.Bookings
            .Include(item => item.User)
            .Include(item => item.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .ThenInclude(hotel => hotel!.Owner)
            .SingleOrDefault(item => item.BookingCode == request.BookingCode.Trim());

        if (booking?.RoomType?.Hotel is null)
        {
            throw new KeyNotFoundException("Booking was not found.");
        }

        if (booking.RoomType.HotelId != hotelId || booking.UserId != userId)
        {
            throw new InvalidOperationException("Booking does not belong to this hotel or user.");
        }

        if (booking.Status != BookingStatuses.Confirmed || booking.PaymentStatus != PaymentStatuses.Paid)
        {
            throw new InvalidOperationException("Chỉ có booking đã thanh toán mới được đánh giá.");
        }

        if (dbContext.HotelReviews.Any(review => review.BookingId == booking.Id))
        {
            throw new InvalidOperationException("Booking này đã được đánh giá.");
        }

        var review = new HotelReview
        {
            HotelId = hotelId,
            BookingId = booking.Id,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim() ?? string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.HotelReviews.Add(review);

        var ownerId = booking.RoomType.Hotel.OwnerUserId;
        if (ownerId is not null)
        {
            AddNotification(
                ownerId.Value,
                "ReviewCreated",
                "Có đánh giá mới",
                $"{booking.User?.FullName ?? booking.GuestName} vừa đánh giá {request.Rating} sao cho {booking.RoomType.Hotel.Name}.",
                $"/public/hotel-detail.html?hotelId={hotelId}");
        }

        AddNotificationForAdmins(
            "ReviewCreated",
            "Có đánh giá khách sạn mới",
            $"{booking.RoomType.Hotel.Name} nhận đánh giá {request.Rating} sao.",
            $"/public/hotel-detail.html?hotelId={hotelId}");

        dbContext.SaveChanges();
        review.Booking = booking;
        review.User = booking.User;
        return ToReviewSummary(review);
    }

    private string CreateBookingCode(DateOnly checkIn)
    {
        var datePart = checkIn.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var nextSequence = dbContext.Bookings.Count(booking => booking.CheckIn == checkIn) + 1;

        while (true)
        {
            var candidate = $"BK{datePart}-{nextSequence:000}";
            if (!dbContext.Bookings.Any(booking => booking.BookingCode == candidate))
            {
                return candidate;
            }

            nextSequence++;
        }
    }

    private BookingConfirmation ToConfirmation(Booking booking, RoomType roomType)
    {
        var hotel = roomType.Hotel
            ?? throw new InvalidOperationException("Room type must include its hotel.");

        var hasReview = dbContext.HotelReviews.Any(review => review.BookingId == booking.Id);

        return new BookingConfirmation(
            BookingCode: booking.BookingCode,
            HotelId: hotel.Id,
            RoomTypeId: booking.RoomTypeId,
            HotelName: hotel.Name,
            RoomTypeName: roomType.Name,
            CheckIn: booking.CheckIn,
            CheckOut: booking.CheckOut,
            Nights: CalculateNights(booking.CheckIn, booking.CheckOut),
            Guests: booking.Guests,
            TotalPrice: booking.TotalPrice,
            Status: booking.Status,
            PaymentStatus: booking.PaymentStatus,
            CanCancel: booking.Status != BookingStatuses.Cancelled && CanCancelBooking(booking),
            CanReview: booking.Status == BookingStatuses.Confirmed
                && booking.PaymentStatus == PaymentStatuses.Paid
                && booking.UserId is not null
                && !hasReview,
            HasReview: hasReview);
    }

    private static int CalculateNights(DateOnly checkIn, DateOnly checkOut)
    {
        return checkOut.DayNumber - checkIn.DayNumber;
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

    private static HotelReviewSummary ToReviewSummary(HotelReview review)
    {
        return new HotelReviewSummary(
            review.Id,
            review.HotelId,
            review.Booking?.BookingCode ?? string.Empty,
            review.User?.FullName ?? review.Booking?.GuestName ?? "Khách hàng",
            review.Rating,
            review.Comment,
            review.CreatedAt);
    }

    private static bool CanCancelBooking(Booking booking)
    {
        var checkInAt = booking.CheckIn.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        return checkInAt > DateTime.UtcNow.AddHours(24);
    }

    private static string NormalizeSort(string? sortBy)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "rating" or "rating_desc" or "top-rated" => "rating",
            "price_desc" or "price-desc" or "highest-price" => "price_desc",
            _ => "price"
        };
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
            BookingId = booking?.Id > 0 ? booking.Id : null,
            Amount = amount,
            BalanceAfter = user.Balance,
            Type = type,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private void CompleteBookingPayment(Booking booking, bool deductPayerBalance)
    {
        var payer = booking.User;
        if (payer is not null && deductPayerBalance)
        {
            if (payer.Balance < booking.TotalPrice)
            {
                throw new InvalidOperationException("So du khong du de thanh toan booking nay.");
            }

            payer.Balance -= booking.TotalPrice;
            AddBalanceTransaction(
                payer,
                booking,
                -booking.TotalPrice,
                "BookingPayment",
                $"Thanh toan booking {booking.BookingCode}");
        }

        var owner = booking.RoomType?.Hotel?.Owner;
        if (owner is not null && owner.Id != payer?.Id)
        {
            owner.Balance += booking.TotalPrice;
            AddBalanceTransaction(
                owner,
                booking,
                booking.TotalPrice,
                "OwnerBookingCredit",
                $"Doanh thu booking {booking.BookingCode}");
        }

        booking.Status = BookingStatuses.Confirmed;
        booking.PaymentStatus = PaymentStatuses.Paid;
        booking.PaidAt = DateTimeOffset.UtcNow;
        AddPaymentNotifications(booking);
    }

    private void AddBookingCreatedNotifications(Booking booking, RoomType roomType)
    {
        var hotel = roomType.Hotel
            ?? throw new InvalidOperationException("Room type must include its hotel.");

        if (booking.UserId is not null)
        {
            AddNotification(
                booking.UserId.Value,
                "BookingCreated",
                "Booking đã được tạo",
                $"Booking {booking.BookingCode} đang chờ thanh toán.",
                $"/public/booking-confirmation.html?code={booking.BookingCode}");
        }

        if (hotel.OwnerUserId is not null)
        {
            AddNotification(
                hotel.OwnerUserId.Value,
                "OwnerNewBooking",
                "Có booking mới",
                $"{booking.GuestName} vừa đặt {roomType.Name} tại {hotel.Name}.",
                "/public/admin.html");
        }

        AddNotificationForAdmins(
            "BookingCreated",
            "Booking mới",
            $"{booking.GuestName} vừa tạo booking {booking.BookingCode} tại {hotel.Name}.",
            "/public/admin.html");
    }

    private void AddPaymentNotifications(Booking booking)
    {
        var hotelName = booking.RoomType?.Hotel?.Name ?? "khách sạn";
        if (booking.UserId is not null)
        {
            AddNotification(
                booking.UserId.Value,
                "BookingPaid",
                "Thanh toán thành công",
                $"Booking {booking.BookingCode} đã thanh toán thành công.",
                $"/public/booking-confirmation.html?code={booking.BookingCode}");
        }

        var ownerId = booking.RoomType?.Hotel?.OwnerUserId;
        if (ownerId is not null && ownerId != booking.UserId)
        {
            AddNotification(
                ownerId.Value,
                "OwnerBookingPaid",
                "Booking đã thanh toán",
                $"Booking {booking.BookingCode} tại {hotelName} đã được thanh toán.",
                "/public/admin.html");
        }

        AddNotificationForAdmins(
            "BookingPaid",
            "Booking đã thanh toán",
            $"Booking {booking.BookingCode} tại {hotelName} đã thanh toán.",
            "/public/admin.html");
    }

    private void AddCancellationNotifications(Booking booking)
    {
        var hotelName = booking.RoomType?.Hotel?.Name ?? "khách sạn";
        if (booking.UserId is not null)
        {
            AddNotification(
                booking.UserId.Value,
                "BookingCancelled",
                "Booking đã hủy",
                $"Booking {booking.BookingCode} đã được hủy.",
                $"/public/booking-confirmation.html?code={booking.BookingCode}");
        }

        var ownerId = booking.RoomType?.Hotel?.OwnerUserId;
        if (ownerId is not null && ownerId != booking.UserId)
        {
            AddNotification(
                ownerId.Value,
                "OwnerBookingCancelled",
                "Booking bị hủy",
                $"Booking {booking.BookingCode} tại {hotelName} đã bị hủy.",
                "/public/admin.html");
        }

        AddNotificationForAdmins(
            "BookingCancelled",
            "Booking đã hủy",
            $"Booking {booking.BookingCode} tại {hotelName} đã hủy.",
            "/public/admin.html");
    }

    private void AddNotificationForAdmins(string type, string title, string message, string linkUrl)
    {
        var adminIds = dbContext.Users
            .Where(user => user.Role == UserRoles.Admin)
            .Select(user => user.Id)
            .ToArray();

        foreach (var adminId in adminIds)
        {
            AddNotification(adminId, type, title, message, linkUrl);
        }
    }

    private void AddNotification(int userId, string type, string title, string message, string linkUrl)
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

    private Booking? FindMutableBooking(string bookingCode, int userId, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(bookingCode))
        {
            return null;
        }

        var query = dbContext.Bookings
            .Include(item => item.User)
            .Include(item => item.PaymentTransactions)
            .Include(item => item.RoomType)
            .ThenInclude(roomType => roomType!.Hotel)
            .ThenInclude(hotel => hotel!.Owner)
            .Where(item => item.BookingCode == bookingCode.Trim());

        if (!isAdmin)
        {
            query = query.Where(item => item.UserId == userId);
        }

        return query
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .FirstOrDefault();
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
        return $"OMNI{Math.Abs(orderCode % 100000):00000}";
    }

    private static string? GetString(JsonElement data, string propertyName)
    {
        if (!data.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.GetRawText();
    }

    private static bool TryGetInt64(JsonElement data, string propertyName, out long value)
    {
        value = 0;
        if (!data.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetInt64(out value);
        }

        return property.ValueKind == JsonValueKind.String
            && long.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryGetDecimal(JsonElement data, string propertyName, out decimal value)
    {
        value = 0;
        if (!data.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetDecimal(out value);
        }

        return property.ValueKind == JsonValueKind.String
            && decimal.TryParse(property.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
