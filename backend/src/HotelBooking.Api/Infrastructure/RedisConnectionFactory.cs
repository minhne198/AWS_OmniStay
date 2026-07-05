using StackExchange.Redis;

namespace HotelBooking.Api.Infrastructure;

public sealed class RedisConnectionFactory(
    IConfiguration configuration,
    ILogger<RedisConnectionFactory> logger) : IDisposable
{
    private readonly string? connectionString = configuration.GetConnectionString("Redis");
    private readonly Lazy<IConnectionMultiplexer?> connection = new(
        () => CreateConnection(configuration.GetConnectionString("Redis"), logger));

    public bool IsConfigured => !string.IsNullOrWhiteSpace(connectionString);

    public IConnectionMultiplexer? TryGetConnection()
    {
        return IsConfigured ? connection.Value : null;
    }

    public void Dispose()
    {
        if (connection.IsValueCreated)
        {
            connection.Value?.Dispose();
        }
    }

    private static IConnectionMultiplexer? CreateConnection(
        string? redisConnectionString,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            return null;
        }

        try
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Redis is configured but the application could not connect.");
            return null;
        }
    }
}
