using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelBooking.Api.Tests;

public class HotelReadTests
{
    [Fact]
    public async Task GetHotel_ReturnsSeededHotelDetails()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        using var response = await client.GetAsync("/api/hotels/10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var hotel = await response.Content.ReadFromJsonAsync<HotelDetails>();
        Assert.NotNull(hotel);
        Assert.Equal(10, hotel.HotelId);
        Assert.Equal("Hoi An Lantern Boutique", hotel.Name);
        Assert.Equal("Hoi An", hotel.City);
        Assert.Equal(4, hotel.StarRating);
        Assert.False(string.IsNullOrWhiteSpace(hotel.Description));
        Assert.False(string.IsNullOrWhiteSpace(hotel.MainImageUrl));
    }

    [Fact]
    public async Task GetHotelRooms_ReturnsThreeRoomTypesForSeededHotel()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        using var response = await client.GetAsync("/api/hotels/10/rooms");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rooms = await response.Content.ReadFromJsonAsync<List<RoomTypeDetails>>();
        Assert.NotNull(rooms);
        Assert.Equal(3, rooms.Count);
        Assert.All(rooms, room =>
        {
            Assert.Equal(10, room.HotelId);
            Assert.True(room.PricePerNight > 0);
            Assert.True(room.TotalRooms > 0);
            Assert.False(string.IsNullOrWhiteSpace(room.ImageUrl));
        });
    }

    [Fact]
    public async Task GetHotel_ReturnsNotFoundForUnknownHotel()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        using var response = await client.GetAsync("/api/hotels/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record HotelDetails(
        int HotelId,
        string Name,
        string City,
        string Address,
        string Description,
        int StarRating,
        string MainImageUrl);

    private sealed record RoomTypeDetails(
        int RoomTypeId,
        int HotelId,
        string Name,
        string Description,
        int MaxGuests,
        decimal PricePerNight,
        int TotalRooms,
        string ImageUrl);
}
