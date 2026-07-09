using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelBooking.Api.Tests;

public class BookingPersistenceTests
{
    [Fact]
    public async Task CreatedBooking_CanBeFetchedFromNewApplicationHostUsingSameDatabaseName()
    {
        var databaseName = $"hotel-booking-{Guid.NewGuid():N}";

        string bookingCode;

        await using (var firstApplication = CreateApplication(databaseName))
        {
            using var firstClient = firstApplication.CreateClient();
            var request = new CreateBookingRequest(
                RoomTypeId: 101,
                GuestName: "Le Thi C",
                GuestEmail: "lethic@example.com",
                CheckIn: new DateOnly(2026, 10, 4),
                CheckOut: new DateOnly(2026, 10, 6),
                Guests: 2);

            using var createResponse = await firstClient.PostAsJsonAsync("/api/bookings", request);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created = await createResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
            Assert.NotNull(created);
            bookingCode = created.BookingCode;
        }

        await using var secondApplication = CreateApplication(databaseName);
        using var secondClient = secondApplication.CreateClient();

        using var fetchResponse = await secondClient.GetAsync($"/api/bookings/{bookingCode}");

        Assert.Equal(HttpStatusCode.OK, fetchResponse.StatusCode);

        var fetched = await fetchResponse.Content.ReadFromJsonAsync<BookingConfirmation>();
        Assert.NotNull(fetched);
        Assert.Equal(bookingCode, fetched.BookingCode);
        Assert.Equal(2_400_000m, fetched.TotalPrice);
    }

    private static WebApplicationFactory<Program> CreateApplication(string databaseName)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:HotelBookingDb", databaseName);
            });
    }

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
