using System.Net;
using System.Net.Http.Json;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Api.Tests;

public class BookingFlowTests
{
    [Fact]
    public async Task SearchHotels_ReturnsAvailableRoomsForCityAndDates()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        using var response = await client.GetAsync(
            "/api/hotels/search?city=Da%20Nang&checkIn=2026-08-15&checkOut=2026-08-18&guests=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<HotelSearchResult>>();
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.All(results, result =>
        {
            Assert.Equal("Da Nang", result.City);
            Assert.True(result.AvailableRooms > 0);
            Assert.True(result.PricePerNight > 0);
            Assert.True(result.MaxGuests >= 2);
        });
    }

    [Fact]
    public async Task SearchHotels_CanFilterByHotelOrRoomName()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        using var response = await client.GetAsync(
            "/api/hotels/search?keyword=Family&checkIn=2026-08-15&checkOut=2026-08-18&guests=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<HotelSearchResult>>();
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.All(results, result =>
        {
            Assert.True(
                result.RoomTypeName.Contains("Family", StringComparison.OrdinalIgnoreCase)
                || result.HotelName.Contains("Family", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public async Task CreateBooking_ReturnsConfirmationAndCanBeFetchedByCode()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        var request = new CreateBookingRequest(
            RoomTypeId: 101,
            GuestName: "Nguyen Van A",
            GuestEmail: "nguyenvana@example.com",
            CheckIn: new DateOnly(2026, 8, 15),
            CheckOut: new DateOnly(2026, 8, 18),
            Guests: 2);

        using var createResponse = await client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var created = await createResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(created);
        Assert.StartsWith("BK", created.BookingCode);
        Assert.Equal("PendingPayment", created.Status);
        Assert.Equal("Pending", created.PaymentStatus);
        Assert.Equal(3_600_000m, created.TotalPrice);

        using var getResponse = await client.GetAsync(createResponse.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(fetched);
        Assert.Equal(created.BookingCode, fetched.BookingCode);
        Assert.Equal(created.TotalPrice, fetched.TotalPrice);
    }

    [Fact]
    public async Task CreateBooking_ReturnsConflictWhenRoomTypeIsSoldOut()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        var request = new CreateBookingRequest(
            RoomTypeId: 103,
            GuestName: "Tran Thi B",
            GuestEmail: "tranthib@example.com",
            CheckIn: new DateOnly(2026, 9, 1),
            CheckOut: new DateOnly(2026, 9, 3),
            Guests: 2);

        using var firstResponse = await client.PostAsJsonAsync("/api/bookings", request);
        using var secondResponse = await client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GetBookingByCode_ReturnsLatestMatchWhenDuplicateCodesExist()
    {
        var databaseName = $"duplicate-code-{Guid.NewGuid():N}";
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", databaseName);
            });

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
            dbContext.Bookings.AddRange(
                new Booking
                {
                    BookingCode = "BK20260724-001",
                    RoomTypeId = 101,
                    GuestName = "Old duplicate",
                    GuestEmail = "old@example.com",
                    CheckIn = new DateOnly(2026, 7, 24),
                    CheckOut = new DateOnly(2026, 7, 25),
                    Guests = 1,
                    TotalPrice = 1_200_000m,
                    Status = "PendingPayment",
                    PaymentStatus = "Pending",
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
                },
                new Booking
                {
                    BookingCode = "BK20260724-001",
                    RoomTypeId = 101,
                    GuestName = "Newest duplicate",
                    GuestEmail = "newest@example.com",
                    CheckIn = new DateOnly(2026, 7, 24),
                    CheckOut = new DateOnly(2026, 7, 26),
                    Guests = 2,
                    TotalPrice = 2_400_000m,
                    Status = "PendingPayment",
                    PaymentStatus = "Pending",
                    CreatedAt = DateTimeOffset.UtcNow
                });
            dbContext.SaveChanges();
        }

        using var client = application.CreateClient();
        using var response = await client.GetAsync("/api/bookings/BK20260724-001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var booking = await response.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(booking);
        Assert.Equal(2, booking.Guests);
        Assert.Equal(2_400_000m, booking.TotalPrice);
    }

    private sealed record HotelSearchResult(
        int HotelId,
        int RoomTypeId,
        string HotelName,
        string City,
        string RoomTypeName,
        int MaxGuests,
        decimal PricePerNight,
        int AvailableRooms);

    private sealed record CreateBookingRequest(
        int RoomTypeId,
        string GuestName,
        string GuestEmail,
        DateOnly CheckIn,
        DateOnly CheckOut,
        int Guests);

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
}
