namespace HotelBooking.Api.Models;

public sealed class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = UserRoles.Customer;

    public DateTimeOffset CreatedAt { get; set; }

    public List<Booking> Bookings { get; set; } = [];
}
