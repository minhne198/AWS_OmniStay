using HotelBooking.Api.Models;

namespace HotelBooking.Api.Data;

public static class HotelBookingSeedData
{
    public static readonly Hotel[] Hotels =
    [
        new()
        {
            Id = 1,
            Name = "Sunrise Da Nang Hotel",
            City = "Da Nang",
            Address = "Vo Nguyen Giap, Son Tra",
            Description = "Beachfront hotel for leisure trips and family stays.",
            StarRating = 4,
            MainImageUrl = "/assets/hotels/sunrise-da-nang.jpg"
        },
        new()
        {
            Id = 2,
            Name = "Saigon Central Stay",
            City = "Ho Chi Minh",
            Address = "Le Loi, District 1",
            Description = "Business-friendly hotel near the city center.",
            StarRating = 4,
            MainImageUrl = "/assets/hotels/saigon-central.jpg"
        },
        new()
        {
            Id = 3,
            Name = "Ha Noi Lake Hotel",
            City = "Ha Noi",
            Address = "Tay Ho",
            Description = "Quiet hotel near the lake with long-stay rooms.",
            StarRating = 3,
            MainImageUrl = "/assets/hotels/ha-noi-lake.jpg"
        },
        new()
        {
            Id = 4,
            Name = "Nha Trang Coral Resort",
            City = "Nha Trang",
            Address = "Tran Phu Beach",
            Description = "Resort close to the beach and island tour piers.",
            StarRating = 4,
            MainImageUrl = "/assets/hotels/nha-trang-coral.jpg"
        },
        new()
        {
            Id = 5,
            Name = "Da Lat Pine Retreat",
            City = "Da Lat",
            Address = "Tuyen Lam Lake",
            Description = "Cool-weather retreat surrounded by pine forests.",
            StarRating = 4,
            MainImageUrl = "/assets/hotels/da-lat-pine.jpg"
        },
        new()
        {
            Id = 6,
            Name = "Phu Quoc Pearl Bay",
            City = "Phu Quoc",
            Address = "Bai Truong",
            Description = "Island resort with family rooms and sunset views.",
            StarRating = 5,
            MainImageUrl = "/assets/hotels/phu-quoc-pearl.jpg"
        },
        new()
        {
            Id = 7,
            Name = "Hue Imperial Garden",
            City = "Hue",
            Address = "Le Loi, Phu Hoi",
            Description = "Boutique hotel near the Perfume River and citadel.",
            StarRating = 4,
            MainImageUrl = "/assets/hotels/hue-imperial.jpg"
        },
        new()
        {
            Id = 8,
            Name = "Sa Pa Cloud Lodge",
            City = "Sa Pa",
            Address = "Muong Hoa",
            Description = "Mountain lodge for trekking trips and cool escapes.",
            StarRating = 3,
            MainImageUrl = "/assets/hotels/sa-pa-cloud.jpg"
        },
        new()
        {
            Id = 9,
            Name = "Quy Nhon Sea Breeze",
            City = "Quy Nhon",
            Address = "Xuan Dieu",
            Description = "Coastal hotel for quiet beach holidays.",
            StarRating = 3,
            MainImageUrl = "/assets/hotels/quy-nhon-sea-breeze.jpg"
        },
        new()
        {
            Id = 10,
            Name = "Hoi An Lantern Boutique",
            City = "Hoi An",
            Address = "Tran Phu, Minh An",
            Description = "Boutique stay near the ancient town and night market.",
            StarRating = 4,
            MainImageUrl = "/assets/hotels/hoi-an-lantern.jpg"
        }
    ];

    public static readonly RoomType[] RoomTypes =
    [
        Room(101, 1, "Deluxe City View", "Comfortable room for two guests with city view.", 2, 1_200_000m, 5, "deluxe-city-view"),
        Room(102, 1, "Family Beach Suite", "Larger suite for family vacations.", 4, 2_400_000m, 3, "family-beach-suite"),
        Room(103, 1, "Last Minute Ocean Room", "Limited room type used to demonstrate overbooking protection.", 2, 1_500_000m, 1, "ocean-room"),
        Room(201, 2, "Business Standard", "Compact room for short business trips.", 2, 950_000m, 8, "business-standard"),
        Room(202, 2, "Executive Corner", "Corner room with desk space and skyline view.", 2, 1_400_000m, 4, "executive-corner"),
        Room(203, 2, "Family City Suite", "Suite for families staying close to District 1.", 4, 2_100_000m, 3, "family-city-suite"),
        Room(301, 3, "Lake View Studio", "Studio room for long stays near West Lake.", 2, 1_050_000m, 6, "lake-view-studio"),
        Room(302, 3, "Old Quarter Deluxe", "Deluxe room with quick access to the Old Quarter.", 2, 1_300_000m, 5, "old-quarter-deluxe"),
        Room(303, 3, "Family Apartment", "Apartment-style room with living area for families.", 4, 1_950_000m, 2, "family-apartment"),
        Room(401, 4, "Coral Standard", "Standard room for beach and island tour travelers.", 2, 1_100_000m, 7, "coral-standard"),
        Room(402, 4, "Ocean Balcony", "Balcony room facing the sea.", 2, 1_700_000m, 5, "ocean-balcony"),
        Room(403, 4, "Premium Family", "Family room with extra beds and resort amenities.", 4, 2_600_000m, 3, "premium-family"),
        Room(501, 5, "Garden Standard", "Quiet garden room for couples.", 2, 900_000m, 6, "garden-standard"),
        Room(502, 5, "Pine View Deluxe", "Deluxe room with pine forest view.", 2, 1_250_000m, 5, "pine-view-deluxe"),
        Room(503, 5, "Fireplace Suite", "Suite with lounge area for cool evenings.", 3, 2_200_000m, 2, "fireplace-suite"),
        Room(601, 6, "Island Standard", "Comfortable island room near the beach.", 2, 1_350_000m, 8, "island-standard"),
        Room(602, 6, "Beachfront Deluxe", "Deluxe room close to the shoreline.", 2, 2_100_000m, 5, "beachfront-deluxe"),
        Room(603, 6, "Pearl Family Villa", "Large villa-style room for family stays.", 5, 3_400_000m, 2, "pearl-family-villa"),
        Room(701, 7, "Classic Heritage", "Classic room inspired by Hue heritage design.", 2, 880_000m, 7, "classic-heritage"),
        Room(702, 7, "River View Deluxe", "Deluxe room with Perfume River view.", 2, 1_300_000m, 4, "river-view-deluxe"),
        Room(703, 7, "Imperial Suite", "Spacious suite for premium city stays.", 3, 2_450_000m, 2, "imperial-suite"),
        Room(801, 8, "Mountain Standard", "Simple room for trekking trips.", 2, 780_000m, 8, "mountain-standard"),
        Room(802, 8, "Cloud View Deluxe", "Deluxe room with valley and cloud views.", 2, 1_150_000m, 5, "cloud-view-deluxe"),
        Room(803, 8, "Family Bungalow", "Bungalow for small families visiting Sa Pa.", 4, 1_800_000m, 3, "family-bungalow"),
        Room(901, 9, "Sea Breeze Standard", "Standard room near the coastal road.", 2, 820_000m, 7, "sea-breeze-standard"),
        Room(902, 9, "Bay View Deluxe", "Deluxe room overlooking the bay.", 2, 1_280_000m, 5, "bay-view-deluxe"),
        Room(903, 9, "Family Sea Suite", "Suite with additional space for families.", 4, 2_050_000m, 3, "family-sea-suite"),
        Room(1001, 10, "Lantern Standard", "Comfortable room near Hoi An ancient town.", 2, 980_000m, 6, "lantern-standard"),
        Room(1002, 10, "Heritage Balcony Deluxe", "Balcony room with heritage-style decor.", 2, 1_550_000m, 4, "heritage-balcony-deluxe"),
        Room(1003, 10, "Family Courtyard Suite", "Courtyard suite for families and longer stays.", 4, 2_350_000m, 3, "family-courtyard-suite")
    ];

    private static RoomType Room(
        int id,
        int hotelId,
        string name,
        string description,
        int maxGuests,
        decimal pricePerNight,
        int totalRooms,
        string imageName)
    {
        return new RoomType
        {
            Id = id,
            HotelId = hotelId,
            Name = name,
            Description = description,
            MaxGuests = maxGuests,
            PricePerNight = pricePerNight,
            TotalRooms = totalRooms,
            ImageUrl = $"/assets/rooms/{imageName}.jpg"
        };
    }
}
