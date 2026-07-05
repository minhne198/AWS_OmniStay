using System.Globalization;
using System.Text.Json;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Infrastructure;
using StackExchange.Redis;

namespace HotelBooking.Api.Services;

public sealed class RedisHotelSearchCache(
    RedisConnectionFactory redisConnectionFactory,
    IConfiguration configuration,
    ILogger<RedisHotelSearchCache> logger) : IHotelSearchCache
{
    private const string KeyPrefix = "omnistay:search:v1";
    private const string KeyIndex = "omnistay:search:v1:keys";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly TimeSpan cacheTtl = TimeSpan.FromSeconds(
        Math.Max(5, configuration.GetValue("Cache:SearchTtlSeconds", 120)));

    public bool TryGet(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests,
        out IReadOnlyList<HotelSearchResult> results)
    {
        results = [];

        var database = TryGetDatabase();
        if (database is null)
        {
            return false;
        }

        try
        {
            var cached = database.StringGet(CreateKey(city, checkIn, checkOut, guests));
            if (cached.IsNullOrEmpty)
            {
                return false;
            }

            results = JsonSerializer.Deserialize<List<HotelSearchResult>>(cached.ToString(), JsonOptions) ?? [];
            return true;
        }
        catch (Exception ex) when (ex is RedisException or JsonException or TimeoutException)
        {
            logger.LogWarning(ex, "Could not read hotel search result from Redis.");
            return false;
        }
    }

    public void Set(
        string city,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests,
        IReadOnlyList<HotelSearchResult> results)
    {
        var database = TryGetDatabase();
        if (database is null)
        {
            return;
        }

        try
        {
            var key = CreateKey(city, checkIn, checkOut, guests);
            var payload = JsonSerializer.Serialize(results, JsonOptions);

            database.StringSet(key, payload, cacheTtl);
            database.SetAdd(KeyIndex, key);
            database.KeyExpire(KeyIndex, cacheTtl.Add(TimeSpan.FromMinutes(5)));
        }
        catch (Exception ex) when (ex is RedisException or JsonException or TimeoutException)
        {
            logger.LogWarning(ex, "Could not write hotel search result to Redis.");
        }
    }

    public void ClearSearchResults()
    {
        var database = TryGetDatabase();
        if (database is null)
        {
            return;
        }

        try
        {
            var keys = database.SetMembers(KeyIndex)
                .Where(value => !value.IsNullOrEmpty)
                .Select(value => (RedisKey)value.ToString())
                .ToArray();

            if (keys.Length > 0)
            {
                database.KeyDelete(keys);
            }

            database.KeyDelete(KeyIndex);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            logger.LogWarning(ex, "Could not clear Redis hotel search cache.");
        }
    }

    private IDatabase? TryGetDatabase()
    {
        var connection = redisConnectionFactory.TryGetConnection();
        return connection?.IsConnected == true ? connection.GetDatabase() : null;
    }

    private static string CreateKey(string city, DateOnly checkIn, DateOnly checkOut, int guests)
    {
        return string.Join(
            ':',
            KeyPrefix,
            Normalize(city),
            checkIn.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            checkOut.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            guests.ToString(CultureInfo.InvariantCulture));
    }

    private static string Normalize(string value)
    {
        return Uri.EscapeDataString(value.Trim().ToUpperInvariant());
    }
}
