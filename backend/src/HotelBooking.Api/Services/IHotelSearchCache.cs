using HotelBooking.Api.Contracts;

namespace HotelBooking.Api.Services;

public interface IHotelSearchCache
{
    bool TryGet(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests,
        out IReadOnlyList<HotelSearchResult> results);

    void Set(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests,
        IReadOnlyList<HotelSearchResult> results);

    void ClearSearchResults();
}
