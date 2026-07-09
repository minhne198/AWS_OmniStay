using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Data;

public sealed class HotelBookingDbContext(DbContextOptions<HotelBookingDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.FullName).HasMaxLength(200).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(200).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(30).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(hotel => hotel.Id);
            entity.Property(hotel => hotel.Name).HasMaxLength(200).IsRequired();
            entity.Property(hotel => hotel.City).HasMaxLength(100).IsRequired();
            entity.Property(hotel => hotel.Address).HasMaxLength(500).IsRequired();
            entity.Property(hotel => hotel.Description).HasMaxLength(2_000).IsRequired();
            entity.Property(hotel => hotel.MainImageUrl).HasMaxLength(500).IsRequired();
            entity.HasIndex(hotel => hotel.City);
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(roomType => roomType.Id);
            entity.Property(roomType => roomType.Name).HasMaxLength(100).IsRequired();
            entity.Property(roomType => roomType.Description).HasMaxLength(2_000).IsRequired();
            entity.Property(roomType => roomType.PricePerNight).HasPrecision(18, 2);
            entity.Property(roomType => roomType.ImageUrl).HasMaxLength(500).IsRequired();
            entity.HasOne(roomType => roomType.Hotel)
                .WithMany(hotel => hotel.RoomTypes)
                .HasForeignKey(roomType => roomType.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(booking => booking.Id);
            entity.Property(booking => booking.BookingCode).HasMaxLength(30).IsRequired();
            entity.Property(booking => booking.GuestName).HasMaxLength(200).IsRequired();
            entity.Property(booking => booking.GuestEmail).HasMaxLength(200).IsRequired();
            entity.Property(booking => booking.Status).HasMaxLength(30).IsRequired();
            entity.Property(booking => booking.PaymentStatus).HasMaxLength(30).IsRequired();
            entity.Property(booking => booking.TotalPrice).HasPrecision(18, 2);
            entity.HasIndex(booking => booking.BookingCode).IsUnique();
            entity.HasIndex(booking => new { booking.RoomTypeId, booking.CheckIn, booking.CheckOut });
            entity.HasOne(booking => booking.RoomType)
                .WithMany(roomType => roomType.Bookings)
                .HasForeignKey(booking => booking.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(booking => booking.User)
                .WithMany(user => user.Bookings)
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Hotel>().HasData(HotelBookingSeedData.Hotels);
        modelBuilder.Entity<RoomType>().HasData(HotelBookingSeedData.RoomTypes);
    }
}
