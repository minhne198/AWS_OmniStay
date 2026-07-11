using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Data;

public sealed class HotelBookingDbContext(DbContextOptions<HotelBookingDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<User> Users => Set<User>();

    public DbSet<HotelReview> HotelReviews => Set<HotelReview>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<BalanceTransaction> BalanceTransactions => Set<BalanceTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.FullName).HasMaxLength(200).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(200).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(30).IsRequired();
            entity.Property(user => user.AvatarUrl).HasMaxLength(500).IsRequired();
            entity.Property(user => user.Balance).HasPrecision(18, 2);
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
            entity.HasOne(hotel => hotel.Owner)
                .WithMany(user => user.OwnedHotels)
                .HasForeignKey(hotel => hotel.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(roomType => roomType.Id);
            entity.Property(roomType => roomType.Name).HasMaxLength(100).IsRequired();
            entity.Property(roomType => roomType.Description).HasMaxLength(2_000).IsRequired();
            entity.Property(roomType => roomType.PricePerNight).HasPrecision(18, 2);
            entity.Property(roomType => roomType.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(roomType => roomType.IsHidden).HasDefaultValue(false);
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

        modelBuilder.Entity<HotelReview>(entity =>
        {
            entity.HasKey(review => review.Id);
            entity.Property(review => review.Comment).HasMaxLength(2_000).IsRequired();
            entity.HasIndex(review => review.HotelId);
            entity.HasIndex(review => review.UserId);
            entity.HasIndex(review => review.BookingId).IsUnique();
            entity.HasOne(review => review.Hotel)
                .WithMany(hotel => hotel.Reviews)
                .HasForeignKey(review => review.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(review => review.Booking)
                .WithOne(booking => booking.Review)
                .HasForeignKey<HotelReview>(review => review.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(review => review.User)
                .WithMany(user => user.HotelReviews)
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(notification => notification.Id);
            entity.Property(notification => notification.Type).HasMaxLength(50).IsRequired();
            entity.Property(notification => notification.Title).HasMaxLength(200).IsRequired();
            entity.Property(notification => notification.Message).HasMaxLength(1_000).IsRequired();
            entity.Property(notification => notification.LinkUrl).HasMaxLength(500).IsRequired();
            entity.HasIndex(notification => new { notification.UserId, notification.IsRead, notification.CreatedAt });
            entity.HasOne(notification => notification.User)
                .WithMany(user => user.Notifications)
                .HasForeignKey(notification => notification.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BalanceTransaction>(entity =>
        {
            entity.HasKey(transaction => transaction.Id);
            entity.Property(transaction => transaction.Amount).HasPrecision(18, 2);
            entity.Property(transaction => transaction.BalanceAfter).HasPrecision(18, 2);
            entity.Property(transaction => transaction.Type).HasMaxLength(50).IsRequired();
            entity.Property(transaction => transaction.Description).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(transaction => new { transaction.UserId, transaction.CreatedAt });
            entity.HasIndex(transaction => transaction.BookingId);
            entity.HasOne(transaction => transaction.User)
                .WithMany(user => user.BalanceTransactions)
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(transaction => transaction.Booking)
                .WithMany(booking => booking.BalanceTransactions)
                .HasForeignKey(transaction => transaction.BookingId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Hotel>().HasData(HotelBookingSeedData.Hotels);
        modelBuilder.Entity<RoomType>().HasData(HotelBookingSeedData.RoomTypes);
    }
}
