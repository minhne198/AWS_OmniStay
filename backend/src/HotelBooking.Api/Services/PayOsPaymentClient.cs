using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HotelBooking.Api.Models;

namespace HotelBooking.Api.Services;

public sealed class PayOsPaymentClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient httpClient;
    private readonly ILogger<PayOsPaymentClient> logger;

    public PayOsPaymentClient(HttpClient httpClient, IConfiguration configuration, ILogger<PayOsPaymentClient> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        Options = new PayOsOptions
        {
            BaseUrl = configuration["PayOS:BaseUrl"] ?? "https://api-merchant.payos.vn",
            ClientId = configuration["PayOS:ClientId"] ?? string.Empty,
            ApiKey = configuration["PayOS:ApiKey"] ?? string.Empty,
            ChecksumKey = configuration["PayOS:ChecksumKey"] ?? string.Empty,
            PartnerCode = configuration["PayOS:PartnerCode"] ?? string.Empty
        };
    }

    public PayOsOptions Options { get; }

    public async Task<PayOsCreatePaymentResult> CreatePaymentLinkAsync(
        PaymentTransaction transaction,
        string returnUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        if (!Options.IsConfigured)
        {
            throw new InvalidOperationException("PayOS is not configured. Set PayOS:ClientId, PayOS:ApiKey and PayOS:ChecksumKey.");
        }

        var amount = ToPayOsAmount(transaction.Amount);
        var signature = CreatePaymentLinkSignature(
            transaction.OrderCode,
            amount,
            transaction.Description,
            returnUrl,
            cancelUrl);

        var requestBody = new PayOsCreatePaymentRequestBody(
            OrderCode: transaction.OrderCode,
            Amount: amount,
            Description: transaction.Description,
            CancelUrl: cancelUrl,
            ReturnUrl: returnUrl,
            Signature: signature,
            ExpiredAt: DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(Options.BaseUrl), "/v2/payment-requests"))
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-client-id", Options.ClientId);
        request.Headers.Add("x-api-key", Options.ApiKey);
        if (!string.IsNullOrWhiteSpace(Options.PartnerCode))
        {
            request.Headers.Add("x-partner-code", Options.PartnerCode);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var payOsResponse = JsonSerializer.Deserialize<PayOsCreatePaymentResponse>(body, JsonOptions);

        if (!response.IsSuccessStatusCode || payOsResponse is null || payOsResponse.Code != "00" || payOsResponse.Data is null)
        {
            var message = payOsResponse?.Desc ?? $"PayOS returned HTTP {(int)response.StatusCode}.";
            logger.LogWarning("PayOS create payment link failed for order {OrderCode}: {Message}", transaction.OrderCode, message);
            throw new InvalidOperationException(message);
        }

        return new PayOsCreatePaymentResult(
            payOsResponse.Data.PaymentLinkId,
            payOsResponse.Data.CheckoutUrl,
            payOsResponse.Data.QrCode);
    }

    public bool IsValidWebhookSignature(JsonElement data, string? signature)
    {
        if (!Options.IsConfigured || string.IsNullOrWhiteSpace(signature) || data.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var signedData = CreateSortedDataString(data);
        var expected = Sign(signedData);
        return FixedTimeEquals(expected, signature);
    }

    public string CreatePaymentLinkSignature(
        long orderCode,
        long amount,
        string description,
        string returnUrl,
        string cancelUrl)
    {
        var data = string.Join(
            '&',
            $"amount={amount.ToString(CultureInfo.InvariantCulture)}",
            $"cancelUrl={cancelUrl}",
            $"description={description}",
            $"orderCode={orderCode.ToString(CultureInfo.InvariantCulture)}",
            $"returnUrl={returnUrl}");
        return Sign(data);
    }

    private string Sign(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Options.ChecksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static long ToPayOsAmount(decimal amount)
    {
        if (amount <= 0 || decimal.Truncate(amount) != amount)
        {
            throw new InvalidOperationException("PayOS amount must be a positive whole VND amount.");
        }

        return decimal.ToInt64(amount);
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected.ToLowerInvariant());
        var actualBytes = Encoding.UTF8.GetBytes(actual.ToLowerInvariant());
        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private static string CreateSortedDataString(JsonElement data)
    {
        return string.Join(
            '&',
            data.EnumerateObject()
                .Where(property => property.Value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => $"{property.Name}={JsonValueString(property.Value)}"));
    }

    private static string JsonValueString(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => value.GetRawText()
        };
    }

    private sealed record PayOsCreatePaymentRequestBody(
        long OrderCode,
        long Amount,
        string Description,
        string CancelUrl,
        string ReturnUrl,
        string Signature,
        long ExpiredAt);

    private sealed record PayOsCreatePaymentResponse(
        string Code,
        string Desc,
        PayOsCreatePaymentData? Data,
        string? Signature);

    private sealed record PayOsCreatePaymentData(
        string PaymentLinkId,
        string CheckoutUrl,
        string QrCode);
}

public sealed record PayOsCreatePaymentResult(
    string PaymentLinkId,
    string CheckoutUrl,
    string QrCode);
