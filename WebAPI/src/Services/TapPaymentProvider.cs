using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WebAPI.DTOs;

namespace WebAPI.Services;

public sealed class TapPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<TapPaymentProvider> _log;

    public TapPaymentProvider(
        HttpClient http,
        ILogger<TapPaymentProvider> log)
    {
        _http = http;
        _log = log;
    }

    public async Task<PaymentResult> ChargeAsync(Guid paymentId, PaymentRequest request, CancellationToken ct)
    {
        var secretKey = Environment.GetEnvironmentVariable("TAP_Test_Secret_Key");

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            return new PaymentResult
            {
                Status = PaymentResultStatus.Failed,
                Error = "Missing Tap secret key"
            };
        }

        var payload = new
        {
            amount = request.Amount,
            currency = request.Currency,
            source = new { id = request.Token },
            customer = new
            {
                first_name = request.FirstName,
                email = request.Email
            },
            redirect = new
            {
                url = $"http://localhost:5045/api/payments/{paymentId}",
            },
            post = new
            {
                url = "https://grimacing-colt-procreate.ngrok-free.dev/api/webhooks/tap"
            },
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.tap.company/v2/charges");
        req.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secretKey);

        req.Content = JsonContent.Create(payload);

        var response = await _http.SendAsync(req, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return new PaymentResult
            {
                Status = PaymentResultStatus.Failed,
                Error = body
            };
        }

        var charge = JsonSerializer.Deserialize<TapChargeResponse>(
            body,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (charge is null)
        {
            return new PaymentResult
            {
                Status = PaymentResultStatus.Failed,
                Error = "Invalid Tap response"
            };
        }

        var status = charge.Status?.ToUpperInvariant();

        return status switch
        {
            "CAPTURED" => new PaymentResult
            {
                Status = PaymentResultStatus.Success,
                TransactionId = charge.Id,
                Provider = "TAP"
            },

            "INITIATED" or "PENDING" => new PaymentResult
            {
                Status = PaymentResultStatus.RequiresAction,
                TransactionId = charge.Id,
                RedirectUrl = charge.Transaction?.Url,
                Provider = "TAP"
            },

            _ => new PaymentResult
            {
                Status = PaymentResultStatus.Failed,
                TransactionId = charge.Id,
                Error = $"Tap status: {charge.Status}",
                Provider = "TAP"
            }
        };
    }
}



public sealed class TapChargeRequest
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = null!;

    [JsonPropertyName("source")]
    public TapSource Source { get; init; } = null!;

    [JsonPropertyName("redirect")]
    public TapRedirect? Redirect { get; init; }

    [JsonPropertyName("customer")]
    public TapCustomer? Customer { get; init; }
}

public sealed class TapSource
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;
}

public sealed class TapRedirect
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = null!;
}

public sealed class TapCustomer
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; init; } = null!;

    [JsonPropertyName("email")]
    public string Email { get; init; } = null!;
}

public sealed class TapChargeResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("transaction")]
    public TapTransaction? Transaction { get; init; }

    [JsonPropertyName("response")]
    public TapGatewayResponse? Response { get; init; }
}

public sealed class TapTransaction
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }
}

public sealed class TapGatewayResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
