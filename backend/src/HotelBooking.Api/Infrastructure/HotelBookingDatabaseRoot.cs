using Microsoft.EntityFrameworkCore.Storage;

namespace HotelBooking.Api.Infrastructure;

public static class HotelBookingDatabaseRoot
{
    public static readonly InMemoryDatabaseRoot Shared = new();
}
