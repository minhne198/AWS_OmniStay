using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HotelBooking.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Api.Tests;

public class AuthAndAdminTests
{
    [Fact]
    public async Task RegisterLoginBookingPaymentAndMyBookings_WorkEndToEnd()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"auth-flow-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var auth = await Register(client, "customer@example.com");
        Assert.Equal(100_000_000m, auth.User.Balance);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        using var createResponse = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(
            RoomTypeId: 101,
            GuestName: "Customer One",
            GuestEmail: "customer@example.com",
            CheckIn: new DateOnly(2026, 11, 10),
            CheckOut: new DateOnly(2026, 11, 12),
            Guests: 2));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(created);
        Assert.Equal("PendingPayment", created.Status);
        Assert.Equal("Pending", created.PaymentStatus);

        var paid = await client.PostAsJsonAsync(
            $"/api/bookings/{created.BookingCode}/pay",
            new MockPaymentRequest("DemoCard"));

        Assert.Equal(HttpStatusCode.OK, paid.StatusCode);
        var paidBooking = await paid.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(paidBooking);
        Assert.Equal("Confirmed", paidBooking.Status);
        Assert.Equal("Paid", paidBooking.PaymentStatus);

        var mine = await client.GetFromJsonAsync<List<BookingConfirmation>>("/api/bookings/my");
        Assert.NotNull(mine);
        Assert.Contains(mine, booking => booking.BookingCode == created.BookingCode);

        var afterPayment = await client.GetFromJsonAsync<UserSummary>("/api/auth/me");
        Assert.NotNull(afterPayment);
        Assert.Equal(100_000_000m - paidBooking.TotalPrice, afterPayment.Balance);
    }

    [Fact]
    public async Task AdminUserEndpoints_RejectCustomerAndAllowSeededAdmin()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"admin-flow-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var customer = await Register(client, "not-admin@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);

        using var forbidden = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var adminLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@omnistay.local",
            "Admin@123456"));
        Assert.Equal(HttpStatusCode.OK, adminLogin.StatusCode);

        var admin = await adminLogin.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        using var allowed = await client.GetAsync("/api/admin/hotels");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);

        var hotels = await allowed.Content.ReadFromJsonAsync<List<AdminHotelSummary>>();
        Assert.NotNull(hotels);
        Assert.NotEmpty(hotels);
    }

    [Fact]
    public async Task AuthenticatedUser_CanManageHotelsAndRoomsForDemo()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"owner-flow-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var owner = await Register(client, "owner@example.com", "HotelOwner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);

        var createHotelResponse = await client.PostAsJsonAsync("/api/admin/hotels", new UpsertHotelRequest(
            "Owner Resort",
            "Da Nang",
            "1 Owner Street",
            "Owner managed property",
            4,
            "https://example.com/hotel.jpg"));
        Assert.Equal(HttpStatusCode.Created, createHotelResponse.StatusCode);
        var createdHotel = await createHotelResponse.Content.ReadFromJsonAsync<AdminHotelSummary>();
        Assert.NotNull(createdHotel);
        Assert.Equal(owner.User.UserId, createdHotel.OwnerUserId);

        using var users = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, users.StatusCode);

        var createRoomResponse = await client.PostAsJsonAsync("/api/admin/room-types", new UpsertRoomTypeRequest(
            createdHotel.HotelId,
            "Owner Suite",
            "Room managed by owner",
            2,
            2_000_000m,
            3,
            "https://example.com/room.jpg"));
        Assert.Equal(HttpStatusCode.Created, createRoomResponse.StatusCode);
        var createdRoom = await createRoomResponse.Content.ReadFromJsonAsync<RoomTypeDetails>();
        Assert.NotNull(createdRoom);

        var profileUpdateResponse = await client.PostAsJsonAsync("/api/auth/me", new UpdateProfileRequest(
            "Updated Owner",
            "/api/uploads/images/demo-owner.jpg"));
        Assert.Equal(HttpStatusCode.OK, profileUpdateResponse.StatusCode);
        var updatedProfile = await profileUpdateResponse.Content.ReadFromJsonAsync<UserSummary>();
        Assert.NotNull(updatedProfile);
        Assert.Equal("Updated Owner", updatedProfile.FullName);
        Assert.Equal("/api/uploads/images/demo-owner.jpg", updatedProfile.AvatarUrl);

        using var postUpdateRoom = await client.PostAsJsonAsync($"/api/admin/room-types/{createdRoom.RoomTypeId}", new UpsertRoomTypeRequest(
            createdHotel.HotelId,
            "Owner Suite Updated",
            "Room updated with POST for CloudFront-friendly demo flow",
            3,
            2_500_000m,
            4,
            "https://example.com/room-updated.jpg"));
        Assert.Equal(HttpStatusCode.OK, postUpdateRoom.StatusCode);

        var otherOwner = await Register(client, "other-owner@example.com", "HotelOwner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherOwner.Token);

        var otherHotels = await client.GetFromJsonAsync<List<AdminHotelSummary>>("/api/admin/hotels");
        Assert.NotNull(otherHotels);
        Assert.DoesNotContain(otherHotels, hotel => hotel.HotelId == createdHotel.HotelId);

        using var updateHotel = await client.PutAsJsonAsync($"/api/admin/hotels/{createdHotel.HotelId}", new UpsertHotelRequest(
            "Hijacked Resort",
            "Da Nang",
            "2 Other Street",
            "Should not update",
            5,
            "https://example.com/hijack.jpg"));
        Assert.Equal(HttpStatusCode.OK, updateHotel.StatusCode);

        using var updateRoom = await client.PutAsJsonAsync($"/api/admin/room-types/{createdRoom.RoomTypeId}", new UpsertRoomTypeRequest(
            createdHotel.HotelId,
            "Hijacked Suite",
            "Should not update",
            2,
            3_000_000m,
            1,
            "https://example.com/hijack-room.jpg"));
        Assert.Equal(HttpStatusCode.OK, updateRoom.StatusCode);
    }

    [Fact]
    public async Task PaidBooking_DeductsCustomerBalanceAndCreditsHotelOwnerBalance()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"balance-flow-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var owner = await Register(client, "balance-owner@example.com", "HotelOwner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);

        var hotelResponse = await client.PostAsJsonAsync("/api/admin/hotels", new UpsertHotelRequest(
            "Balance Resort",
            "Da Nang",
            "1 Balance Street",
            "Owner payout test",
            4,
            "https://example.com/balance-hotel.jpg"));
        var hotel = await hotelResponse.Content.ReadFromJsonAsync<AdminHotelSummary>();
        Assert.NotNull(hotel);

        var roomResponse = await client.PostAsJsonAsync("/api/admin/room-types", new UpsertRoomTypeRequest(
            hotel.HotelId,
            "Balance Room",
            "Customer payment test",
            2,
            2_500_000m,
            2,
            "https://example.com/balance-room.jpg"));
        var room = await roomResponse.Content.ReadFromJsonAsync<RoomTypeDetails>();
        Assert.NotNull(room);

        var customer = await Register(client, "balance-customer@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);

        var bookingResponse = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(
            room.RoomTypeId,
            "Balance Customer",
            "balance-customer@example.com",
            new DateOnly(2026, 12, 10),
            new DateOnly(2026, 12, 12),
            2));
        Assert.Equal(HttpStatusCode.Created, bookingResponse.StatusCode);

        var booking = await bookingResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(booking);
        Assert.Equal(5_000_000m, booking.TotalPrice);

        using var paymentResponse = await client.PostAsJsonAsync(
            $"/api/bookings/{booking.BookingCode}/pay",
            new MockPaymentRequest("DemoCard"));
        Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);

        var customerAfterPayment = await client.GetFromJsonAsync<UserSummary>("/api/auth/me");
        Assert.NotNull(customerAfterPayment);
        Assert.Equal(95_000_000m, customerAfterPayment.Balance);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var ownerAfterPayment = await client.GetFromJsonAsync<UserSummary>("/api/auth/me");
        Assert.NotNull(ownerAfterPayment);
        Assert.Equal(105_000_000m, ownerAfterPayment.Balance);
    }

    [Fact]
    public async Task HotelOwner_CanViewBookingsForOwnedRooms()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"owner-bookings-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var owner = await Register(client, "booking-owner@example.com", "HotelOwner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);

        var hotelResponse = await client.PostAsJsonAsync("/api/admin/hotels", new UpsertHotelRequest(
            "Owner Booking Hotel",
            "Da Nang",
            "1 Booking Street",
            "Owner booking visibility test",
            4,
            "https://example.com/hotel.jpg"));
        var hotel = await hotelResponse.Content.ReadFromJsonAsync<AdminHotelSummary>();
        Assert.NotNull(hotel);

        var roomResponse = await client.PostAsJsonAsync("/api/admin/room-types", new UpsertRoomTypeRequest(
            hotel.HotelId,
            "Booking Room",
            "Visible to owner",
            2,
            1_500_000m,
            2,
            "https://example.com/room.jpg"));
        var room = await roomResponse.Content.ReadFromJsonAsync<RoomTypeDetails>();
        Assert.NotNull(room);

        var customer = await Register(client, "owner-booking-customer@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);

        var bookingResponse = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(
            room.RoomTypeId,
            "Owner Booking Customer",
            "owner-booking-customer@example.com",
            new DateOnly(2026, 12, 22),
            new DateOnly(2026, 12, 24),
            2));
        Assert.Equal(HttpStatusCode.Created, bookingResponse.StatusCode);
        var booking = await bookingResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(booking);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var ownerBookings = await client.GetFromJsonAsync<List<AdminBookingSummary>>("/api/admin/bookings");
        Assert.NotNull(ownerBookings);
        var visibleBooking = Assert.Single(ownerBookings, item => item.BookingCode == booking.BookingCode);
        Assert.Equal("Owner Booking Customer", visibleBooking.GuestName);
        Assert.Equal("owner-booking-customer@example.com", visibleBooking.GuestEmail);
    }

    [Fact]
    public async Task HiddenRoom_IsNotPubliclySearchableOrBookable()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"hidden-room-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var owner = await Register(client, "hidden-owner@example.com", "HotelOwner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);

        var hotelResponse = await client.PostAsJsonAsync("/api/admin/hotels", new UpsertHotelRequest(
            "Hidden Hotel",
            "Da Nang",
            "1 Hidden Street",
            "Hidden room test",
            4,
            "https://example.com/hotel.jpg"));
        var hotel = await hotelResponse.Content.ReadFromJsonAsync<AdminHotelSummary>();
        Assert.NotNull(hotel);

        var roomResponse = await client.PostAsJsonAsync("/api/admin/room-types", new UpsertRoomTypeRequest(
            hotel.HotelId,
            "Hidden Room",
            "Should not be public",
            2,
            1_000_000m,
            2,
            "https://example.com/room.jpg",
            true));
        var room = await roomResponse.Content.ReadFromJsonAsync<RoomTypeDetails>();
        Assert.NotNull(room);
        Assert.True(room.IsHidden);

        using var searchResponse = await client.GetAsync(
            "/api/hotels/search?city=Da%20Nang&keyword=Hidden&checkIn=2026-12-22&checkOut=2026-12-24&guests=2");
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var searchResults = await searchResponse.Content.ReadFromJsonAsync<List<HotelSearchResult>>();
        Assert.NotNull(searchResults);
        Assert.DoesNotContain(searchResults, item => item.RoomTypeId == room.RoomTypeId);

        var rooms = await client.GetFromJsonAsync<List<RoomTypeDetails>>($"/api/hotels/{hotel.HotelId}/rooms");
        Assert.NotNull(rooms);
        Assert.DoesNotContain(rooms, item => item.RoomTypeId == room.RoomTypeId);

        var customer = await Register(client, "hidden-customer@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);
        using var bookingResponse = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(
            room.RoomTypeId,
            "Hidden Customer",
            "hidden-customer@example.com",
            new DateOnly(2026, 12, 22),
            new DateOnly(2026, 12, 24),
            2));
        Assert.Equal(HttpStatusCode.NotFound, bookingResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_CanEditUserBalance()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"admin-balance-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@omnistay.local",
            "Admin@123456"));
        var admin = await adminLogin.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var createUser = await client.PostAsJsonAsync("/api/admin/users", new UpsertUserRequest(
            "Balance User",
            "admin-balance-user@example.com",
            "Password123!",
            "Customer",
            10_000_000m,
            string.Empty));
        Assert.Equal(HttpStatusCode.Created, createUser.StatusCode);
        var user = await createUser.Content.ReadFromJsonAsync<AdminUserSummary>();
        Assert.NotNull(user);
        Assert.Equal(10_000_000m, user.Balance);

        var updateUser = await client.PutAsJsonAsync($"/api/admin/users/{user.UserId}", new UpsertUserRequest(
            "Balance User",
            "admin-balance-user@example.com",
            string.Empty,
            "Customer",
            123_456_789m,
            string.Empty));
        Assert.Equal(HttpStatusCode.OK, updateUser.StatusCode);
        var updated = await updateUser.Content.ReadFromJsonAsync<AdminUserSummary>();
        Assert.NotNull(updated);
        Assert.Equal(123_456_789m, updated.Balance);
    }

    [Fact]
    public async Task Admin_CannotRemoveOwnAdminRoleOrDeleteOwnAccount()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"admin-self-protect-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@omnistay.local",
            "Admin@123456"));
        var admin = await adminLogin.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        using var demoteResponse = await client.PutAsJsonAsync(
            $"/api/admin/users/{admin.User.UserId}",
            new UpsertUserRequest(
                admin.User.FullName,
                admin.User.Email,
                string.Empty,
                "Customer",
                admin.User.Balance,
                admin.User.AvatarUrl));
        Assert.Equal(HttpStatusCode.BadRequest, demoteResponse.StatusCode);

        using var deleteResponse = await client.DeleteAsync($"/api/admin/users/{admin.User.UserId}");
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

        var refreshedAdmin = await client.GetFromJsonAsync<UserSummary>("/api/auth/me");
        Assert.NotNull(refreshedAdmin);
        Assert.Equal("Admin", refreshedAdmin.Role);
    }

    [Fact]
    public async Task Refresh_ReturnsTokenForCurrentDatabaseRole()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"refresh-role-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var customer = await Register(client, "refresh-role@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
            var user = dbContext.Users.Single(item => item.Id == customer.User.UserId);
            user.Role = "HotelOwner";
            dbContext.SaveChanges();
        }

        var refreshResponse = await client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(refreshed);
        Assert.Equal("HotelOwner", refreshed.User.Role);
        Assert.NotEqual(customer.Token, refreshed.Token);
    }

    [Fact]
    public async Task ReviewsNotificationsDashboardTransactionsRefundAndOwnerProfile_WorkTogether()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"growth-flow-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var owner = await Register(client, "growth-owner@example.com", "HotelOwner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);

        var hotelResponse = await client.PostAsJsonAsync("/api/admin/hotels", new UpsertHotelRequest(
            "Review Growth Hotel",
            "Da Nang",
            "1 Growth Street",
            "Hotel for review and dashboard flow",
            5,
            "https://example.com/growth-hotel.jpg"));
        var hotel = await hotelResponse.Content.ReadFromJsonAsync<AdminHotelSummary>();
        Assert.NotNull(hotel);

        var roomResponse = await client.PostAsJsonAsync("/api/admin/room-types", new UpsertRoomTypeRequest(
            hotel.HotelId,
            "Growth Room",
            "Room for growth flow",
            2,
            2_000_000m,
            4,
            "https://example.com/growth-room.jpg"));
        var room = await roomResponse.Content.ReadFromJsonAsync<RoomTypeDetails>();
        Assert.NotNull(room);

        var customer = await Register(client, "growth-customer@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);

        var bookingResponse = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(
            room.RoomTypeId,
            "Growth Customer",
            "growth-customer@example.com",
            new DateOnly(2026, 12, 28),
            new DateOnly(2026, 12, 30),
            2));
        Assert.Equal(HttpStatusCode.Created, bookingResponse.StatusCode);
        var booking = await bookingResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(booking);
        Assert.Equal(4_000_000m, booking.TotalPrice);

        var paidResponse = await client.PostAsJsonAsync(
            $"/api/bookings/{booking.BookingCode}/pay",
            new MockPaymentRequest("DemoCard"));
        Assert.Equal(HttpStatusCode.OK, paidResponse.StatusCode);

        var customerNotifications = await client.GetFromJsonAsync<List<NotificationSummary>>("/api/notifications/my");
        Assert.NotNull(customerNotifications);
        Assert.Contains(customerNotifications, item => item.Type == "BookingPaid");

        var reviewResponse = await client.PostAsJsonAsync($"/api/hotels/{hotel.HotelId}/reviews", new CreateHotelReviewRequest(
            booking.BookingCode,
            5,
            "Phong sach va dich vu tot."));
        Assert.Equal(HttpStatusCode.Created, reviewResponse.StatusCode);

        var searchResults = await client.GetFromJsonAsync<List<HotelSearchResult>>(
            "/api/hotels/search?city=Da%20Nang&keyword=Review%20Growth&checkIn=2026-12-28&checkOut=2026-12-30&guests=2&minRating=5&sortBy=rating");
        Assert.NotNull(searchResults);
        var reviewedResult = Assert.Single(searchResults, item => item.HotelId == hotel.HotelId);
        Assert.Equal(5, reviewedResult.AverageRating);
        Assert.Equal(1, reviewedResult.ReviewCount);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var ownerNotifications = await client.GetFromJsonAsync<List<NotificationSummary>>("/api/notifications/my");
        Assert.NotNull(ownerNotifications);
        Assert.Contains(ownerNotifications, item => item.Type == "OwnerNewBooking");

        var ownerProfileBeforeCancel = await client.GetFromJsonAsync<OwnerProfileSummary>("/api/account/owner-profile");
        Assert.NotNull(ownerProfileBeforeCancel);
        Assert.Equal("Verified", ownerProfileBeforeCancel.VerificationStatus);
        Assert.Equal(4_000_000m, ownerProfileBeforeCancel.TotalRevenue);
        Assert.Contains(ownerProfileBeforeCancel.Hotels, item => item.HotelId == hotel.HotelId && item.ReviewCount == 1);

        var adminLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@omnistay.local",
            "Admin@123456"));
        var admin = await adminLogin.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var dashboard = await client.GetFromJsonAsync<DashboardSummary>("/api/admin/dashboard");
        Assert.NotNull(dashboard);
        Assert.Equal(4_000_000m, dashboard.TotalRevenue);
        Assert.Equal("Growth Room", dashboard.MostBookedRoomName);

        var transactions = await client.GetFromJsonAsync<List<BalanceTransactionSummary>>("/api/admin/balance-transactions");
        Assert.NotNull(transactions);
        Assert.Contains(transactions, item => item.BookingCode == booking.BookingCode && item.Type == "BookingPayment");
        Assert.Contains(transactions, item => item.BookingCode == booking.BookingCode && item.Type == "OwnerBookingCredit");

        var activity = await client.GetFromJsonAsync<List<AdminActivitySummary>>("/api/admin/activity");
        Assert.NotNull(activity);
        Assert.Contains(activity, item => item.Type == "BookingPaid");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);
        var cancelResponse = await client.DeleteAsync($"/api/bookings/{booking.BookingCode}");
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(cancelled);
        Assert.Equal("Cancelled", cancelled.Status);
        Assert.Equal("Refunded", cancelled.PaymentStatus);

        var customerAfterRefund = await client.GetFromJsonAsync<UserSummary>("/api/auth/me");
        Assert.NotNull(customerAfterRefund);
        Assert.Equal(100_000_000m, customerAfterRefund.Balance);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var ownerAfterRefund = await client.GetFromJsonAsync<UserSummary>("/api/auth/me");
        Assert.NotNull(ownerAfterRefund);
        Assert.Equal(100_000_000m, ownerAfterRefund.Balance);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);
        var transactionsAfterRefund = await client.GetFromJsonAsync<List<BalanceTransactionSummary>>("/api/admin/balance-transactions");
        Assert.NotNull(transactionsAfterRefund);
        Assert.Contains(transactionsAfterRefund, item => item.BookingCode == booking.BookingCode && item.Type == "BookingRefund");
        Assert.Contains(transactionsAfterRefund, item => item.BookingCode == booking.BookingCode && item.Type == "OwnerBookingReversal");
    }

    [Fact]
    public async Task Register_DoesNotAllowSelfRegisteringAdmin()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"admin-register-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Sneaky Admin",
            "sneaky@example.com",
            "Password123!",
            "Admin"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<AuthResponse> Register(HttpClient client, string email, string? role = null)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Test User",
            email,
            "Password123!",
            role));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        return auth;
    }

    private sealed record RegisterRequest(string FullName, string Email, string Password, string? Role = null);

    private sealed record LoginRequest(string Email, string Password);

    private sealed record AuthResponse(string Token, UserSummary User);

    private sealed record UserSummary(
        int UserId,
        string FullName,
        string Email,
        string Role,
        string AvatarUrl,
        decimal Balance);

    private sealed record UpdateProfileRequest(
        string FullName,
        string? AvatarUrl);

    private sealed record CreateBookingRequest(
        int RoomTypeId,
        string GuestName,
        string GuestEmail,
        DateOnly CheckIn,
        DateOnly CheckOut,
        int Guests);

    private sealed record MockPaymentRequest(string PaymentMethod);

    private sealed record BookingConfirmation(
        string BookingCode,
        int RoomTypeId,
        string HotelName,
        string RoomTypeName,
        DateOnly CheckIn,
        DateOnly CheckOut,
        int Nights,
        int Guests,
        decimal TotalPrice,
        string Status,
        string PaymentStatus);

    private sealed record AdminHotelSummary(
        int HotelId,
        int? OwnerUserId,
        string Name,
        string City,
        string Address,
        string Description,
        int StarRating,
        string MainImageUrl,
        int RoomTypeCount);

    private sealed record UpsertHotelRequest(
        string Name,
        string City,
        string Address,
        string Description,
        int StarRating,
        string MainImageUrl);

    private sealed record UpsertRoomTypeRequest(
        int HotelId,
        string Name,
        string Description,
        int MaxGuests,
        decimal PricePerNight,
        int TotalRooms,
        string ImageUrl,
        bool IsHidden = false);

    private sealed record RoomTypeDetails(
        int RoomTypeId,
        int HotelId,
        string Name,
        string Description,
        int MaxGuests,
        decimal PricePerNight,
        int TotalRooms,
        string ImageUrl,
        bool IsHidden,
        int BookedRooms,
        int AvailableRooms);

    private sealed record UpsertUserRequest(
        string FullName,
        string Email,
        string? Password,
        string Role,
        decimal? Balance,
        string? AvatarUrl);

    private sealed record AdminUserSummary(
        int UserId,
        string FullName,
        string Email,
        string Role,
        string AvatarUrl,
        decimal Balance,
        DateTimeOffset CreatedAt);

    private sealed record AdminBookingSummary(
        string BookingCode,
        string GuestName,
        string GuestEmail,
        string? UserEmail,
        string HotelName,
        string RoomTypeName,
        DateOnly CheckIn,
        DateOnly CheckOut,
        int Guests,
        decimal TotalPrice,
        string Status,
        string PaymentStatus,
        DateTimeOffset CreatedAt);

    private sealed record HotelSearchResult(
        int HotelId,
        int RoomTypeId,
        string HotelName,
        string City,
        string RoomTypeName,
        string MainImageUrl,
        string RoomImageUrl,
        int MaxGuests,
        decimal PricePerNight,
        int AvailableRooms,
        double AverageRating,
        int ReviewCount);

    private sealed record CreateHotelReviewRequest(
        string BookingCode,
        int Rating,
        string? Comment);

    private sealed record NotificationSummary(
        int NotificationId,
        string Type,
        string Title,
        string Message,
        string LinkUrl,
        bool IsRead,
        DateTimeOffset CreatedAt);

    private sealed record RevenuePoint(
        string Label,
        decimal Revenue,
        int BookingCount);

    private sealed record DashboardSummary(
        decimal TotalRevenue,
        int BookingCount,
        string MostBookedRoomName,
        int MostBookedRoomBookings,
        int TotalRooms,
        int BookedRooms,
        decimal OccupancyRate,
        IReadOnlyList<RevenuePoint> RevenueByDay,
        IReadOnlyList<RevenuePoint> RevenueByMonth);

    private sealed record BalanceTransactionSummary(
        int TransactionId,
        int UserId,
        string UserEmail,
        string UserFullName,
        string? BookingCode,
        decimal Amount,
        decimal BalanceAfter,
        string Type,
        string Description,
        DateTimeOffset CreatedAt);

    private sealed record AdminActivitySummary(
        int NotificationId,
        int UserId,
        string UserEmail,
        string Type,
        string Title,
        string Message,
        string LinkUrl,
        DateTimeOffset CreatedAt);

    private sealed record OwnerHotelProfileSummary(
        int HotelId,
        string Name,
        string City,
        string MainImageUrl,
        int RoomTypeCount,
        decimal TotalRevenue,
        int BookingCount,
        double AverageRating,
        int ReviewCount);

    private sealed record OwnerProfileSummary(
        int OwnerUserId,
        string FullName,
        string Email,
        string AvatarUrl,
        string VerificationStatus,
        decimal TotalRevenue,
        int OwnedHotelCount,
        IReadOnlyList<OwnerHotelProfileSummary> Hotels);
}
