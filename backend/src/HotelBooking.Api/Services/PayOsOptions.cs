namespace HotelBooking.Api.Services;

public sealed class PayOsOptions
{
    public string BaseUrl { get; init; } = "https://api-merchant.payos.vn";

    public string ClientId { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public string ChecksumKey { get; init; } = string.Empty;

    public string PartnerCode { get; init; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ApiKey)
        && !string.IsNullOrWhiteSpace(ChecksumKey);
}
