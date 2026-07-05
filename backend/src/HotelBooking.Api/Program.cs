using HotelBooking.Api.Data;
using HotelBooking.Api.Infrastructure;
using HotelBooking.Api.Services;
using Microsoft.EntityFrameworkCore;

const string LocalFrontendCorsPolicy = "LocalFrontend";
const string InMemoryDatabaseProvider = "InMemory";
const string MySqlDatabaseProvider = "MySql";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ??
        [
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "http://localhost:5500",
            "http://127.0.0.1:5500"
        ];

    options.AddPolicy(
        LocalFrontendCorsPolicy,
        policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddDbContext<HotelBookingDbContext>(options =>
{
    var databaseProvider = builder.Configuration["Database:Provider"] ?? InMemoryDatabaseProvider;

    if (databaseProvider.Equals(MySqlDatabaseProvider, StringComparison.OrdinalIgnoreCase))
    {
        var connectionString = builder.Configuration.GetConnectionString("HotelBookingDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:HotelBookingDb is required when Database:Provider is MySql.");
        }

        options.UseMySQL(connectionString);
        return;
    }

    var databaseName = builder.Configuration.GetConnectionString("HotelBookingDb")
        ?? "HotelBookingLocal";

    options.UseInMemoryDatabase(databaseName, HotelBookingDatabaseRoot.Shared);
});
builder.Services.AddSingleton<RedisConnectionFactory>();
builder.Services.AddSingleton<IHotelSearchCache, RedisHotelSearchCache>();
builder.Services.AddScoped<EfCoreHotelBookingService>();
builder.Services.AddScoped<IHotelBookingService, CachedHotelBookingService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok())
    .WithName("HealthCheck");

app.MapGet("/health/aws", (
        IConfiguration configuration,
        IWebHostEnvironment environment,
        RedisConnectionFactory redisConnectionFactory) =>
    {
        var redisConnection = redisConnectionFactory.TryGetConnection();

        return Results.Ok(new AwsRuntimeStatus(
            Environment: environment.EnvironmentName,
            Region: configuration["AWS:Region"] ?? configuration["AWS_REGION"] ?? "not-configured",
            DatabaseProvider: configuration["Database:Provider"] ?? InMemoryDatabaseProvider,
            RedisConfigured: redisConnectionFactory.IsConfigured,
            RedisConnected: redisConnection?.IsConnected ?? false,
            SearchCacheTtlSeconds: configuration.GetValue("Cache:SearchTtlSeconds", 120),
            S3FrontendBucket: configuration["AWS:S3FrontendBucket"] ?? "not-configured",
            CloudFrontDomain: configuration["AWS:CloudFrontDomain"] ?? "not-configured",
            ApiBasePath: "/api"));
    })
    .WithName("AwsRuntimeStatus");

app.UseCors(LocalFrontendCorsPolicy);
app.MapControllers();

app.Run();

public partial class Program;

internal sealed record AwsRuntimeStatus(
    string Environment,
    string Region,
    string DatabaseProvider,
    bool RedisConfigured,
    bool RedisConnected,
    int SearchCacheTtlSeconds,
    string S3FrontendBucket,
    string CloudFrontDomain,
    string ApiBasePath);
