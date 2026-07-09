using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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
    }

    [Fact]
    public async Task AdminEndpoints_RejectCustomerAndAllowSeededAdmin()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", $"admin-flow-{Guid.NewGuid():N}");
            });
        using var client = application.CreateClient();

        var customer = await Register(client, "not-admin@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);

        using var forbidden = await client.GetAsync("/api/admin/bookings");
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

    private static async Task<AuthResponse> Register(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Test User",
            email,
            "Password123!"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        return auth;
    }

    private sealed record RegisterRequest(string FullName, string Email, string Password);

    private sealed record LoginRequest(string Email, string Password);

    private sealed record AuthResponse(string Token, UserSummary User);

    private sealed record UserSummary(int UserId, string FullName, string Email, string Role);

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
        string Name,
        string City,
        string Address,
        string Description,
        int StarRating,
        string MainImageUrl,
        int RoomTypeCount);
}
