using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelBooking.Api.Tests;

public class HealthEndpointTests
{
    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAwsHealth_ReturnsRuntimeConfigurationSummary()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        using var response = await client.GetAsync("/health/aws");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var status = await response.Content.ReadFromJsonAsync<AwsRuntimeStatus>();
        Assert.NotNull(status);
        Assert.Equal("InMemory", status.DatabaseProvider);
        Assert.False(status.RedisConfigured);
        Assert.False(status.RedisConnected);
        Assert.Equal("/api", status.ApiBasePath);
    }

    private sealed record AwsRuntimeStatus(
        string Environment,
        string Region,
        string DatabaseProvider,
        bool RedisConfigured,
        bool RedisConnected,
        int SearchCacheTtlSeconds,
        string S3FrontendBucket,
        string CloudFrontDomain,
        string ApiBasePath);
}
