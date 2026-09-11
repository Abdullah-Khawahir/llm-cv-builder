using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/webhooks/tap")]
public class TapWebhookController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<TapWebhookController> _log;
    private readonly string _tapWebhookSecret;

    public TapWebhookController(
        AppDbContext db,
        ILogger<TapWebhookController> log,
        IConfiguration config)
    {
        _db = db;
        _log = log;
        _tapWebhookSecret = Environment.GetEnvironmentVariable("TAP_Test_Secret_Key")!;
    }

    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] TapWebhookBody payload)
    {
        var now = DateTime.UtcNow;

        _log.LogInformation("Tap Webhook received: {Id}, {Status}, {Amount} {Currency}",
            payload?.Id, payload?.Status, payload?.Amount, payload?.Currency);

        // 1. Validate request
        var validation = ValidateRequest(payload);
        if (validation != null)
            return validation;

        // 2. Load payment
        var payment = await GetPayment(payload!.Id);
        if (payment == null)
            return NotFound();

        // 3. Idempotency
        if (IsAlreadyProcessed(payment))
            return Ok();

        // 4. Received event
        AddEvent(payment,
            PaymentEventType.WebhookReceived,
            "Webhook received successfully.",
            payload,
            now);

        // 5. Signature validation
        if (ValidateWebhookSignature(payload) is { } unauthorized)
            return unauthorized;

        // 6. Verified event
        AddEvent(payment,
            PaymentEventType.WebhookVerified,
            "Webhook signature verified.",
            payload,
            now);

        // 7. State machine
        HandleStatusTransition(payment, payload, now);

        // 8. Save
        await PersistChanges(payment);

        return Ok();
    }

    private IActionResult? ValidateRequest(TapWebhookBody? payload)
    {
        if (payload == null || string.IsNullOrEmpty(payload.Id))
        {
            _log.LogWarning("Invalid webhook payload.");
            return BadRequest("Invalid payload.");
        }

        return null;
    }
    private string GetFormattedAmount(decimal amount, string currency)
    {
        int decimalPlaces = currency?.ToUpperInvariant() switch
        {
            "BHD" or "KWD" or "OMR" or "JOD" => 3,
            _ => 2 // Default to 2 for SAR, AED, USD, EUR, GBP, QAR, EGP, etc.
        };

        return amount.ToString($"F{decimalPlaces}", System.Globalization.CultureInfo.InvariantCulture);
    }
    private async Task<Payment?> GetPayment(string id)
    {
        _log.LogInformation("Searching payment: {Id}", id);

        var payment = await _db.Payments
            .Include(p => p.Events)
            .FirstOrDefaultAsync(p => p.ProviderRef == id);

        if (payment == null)
            _log.LogWarning("Payment not found: {Id}", id);

        return payment;
    }

    private bool IsAlreadyProcessed(Payment payment)
    {
        bool done = payment.Events.Any(e =>
            e.Type == PaymentEventType.ChargeSucceeded ||
            e.Type == PaymentEventType.ChargeFailed);

        if (done)
            _log.LogInformation("Payment already processed: {Id}", payment.Id);

        return done;
    }

    private void AddEvent(
        Payment payment,
        PaymentEventType type,
        string message,
        TapWebhookBody payload,
        DateTime now)
    {
        _log.LogInformation("Adding event {Type} for payment {PaymentId}", type, payment.Id);
        _db.PaymentEvents.Add(new PaymentEvent
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Type = type,
            Message = message,
            RawPayload = JsonSerializer.Serialize(payload),
            CreatedAt = now
        });
    }

    private IActionResult? ValidateWebhookSignature(TapWebhookBody payload)
    {
        if (!Request.Headers.TryGetValue("hashstring", out var signature))
        {
            _log.LogWarning("Missing signature header: {Id}", payload.Id);
            return Unauthorized("Missing signature.");
        }

        if (IsSignatureInvalid(payload, signature.ToString()))
        {
            _log.LogWarning("Invalid signature: {Id}", payload.Id);
            return Unauthorized("Invalid signature.");
        }

        _log.LogInformation("Signature verified: {Id}", payload.Id);
        return null;
    }

    private bool IsSignatureInvalid(TapWebhookBody payload, string providedSignature)
    {
        // Corrected path to Created based on your provided JSON
        string id = payload.Id ?? "";
        string amount = GetFormattedAmount(payload.Amount, payload.Currency);
        string currency = payload.Currency ?? "";

        // Ensure we handle potential nulls if the field is missing
        string gatewayRef = payload.Reference?.Gateway ?? "";
        string paymentRef = payload.Reference?.Payment ?? "";
        string status = payload.Status ?? "";

        // Accessing the nested 'date' object
        string created = payload.Transaction?.Date?.Created.ToString() ?? "";

        string dataToHash =
            $"x_id{id}x_amount{amount}x_currency{currency}" +
            $"x_gateway_reference{gatewayRef}x_payment_reference{paymentRef}" +
            $"x_status{status}x_created{created}";

        // Debug log to confirm what you are hashing
        _log.LogWarning("Final string being hashed: {Data}", dataToHash);

        var secret = _tapWebhookSecret.Trim();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
        var computed = Convert.ToHexString(hash).ToLowerInvariant();

        return !string.Equals(computed, providedSignature, StringComparison.OrdinalIgnoreCase);
    }

    // =========================
    // State machine
    // =========================
    private void HandleStatusTransition(Payment payment, TapWebhookBody payload, DateTime now)
    {
        var status = payload.Status?.ToUpperInvariant();

        _log.LogInformation("Processing status {Status} for {PaymentId}", status, payment.Id);

        switch (status)
        {
            case "CAPTURED":
                payment.UpdatedAt = now;
                _log.LogInformation("Payment captured: {PaymentId}", payment.Id);
                AddEvent(payment,
                    PaymentEventType.ChargeSucceeded,
                    $"Captured {payload.Amount} {payload.Currency}",
                    payload,
                    now);
                break;

            case "FAILED":
            case "DECLINED":
            case "CANCELLED":
            case "VOID":
                var reason = payload.Response?.Message ?? "Gateway failure";

                payment.FailureReason = reason;
                payment.UpdatedAt = now;
                _log.LogInformation("Payment failed: {PaymentId}, reason: {Reason}", payment.Id, reason);
                AddEvent(payment,
                    PaymentEventType.ChargeFailed,
                    $"Failed: {reason}",
                    payload,
                    now);
                break;

            default:
                _log.LogInformation("Ignored status {Status}", payload.Status);
                break;
        }
    }

    // =========================
    // Persistence
    // =========================
    private async Task PersistChanges(Payment payment)
    {
        _log.LogInformation("Saving payment {PaymentId}", payment.Id);

        await _db.SaveChangesAsync();

        _log.LogInformation("Saved payment {PaymentId}", payment.Id);
    }
}

public record TapWebhookBody
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("live_mode")]
    public bool LiveMode { get; set; } = default!;

    [JsonPropertyName("customer_initiated")]
    public bool CustomerInitiated { get; set; } = default!;

    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; } = default!;

    [JsonPropertyName("method")]
    public string Method { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; } = default!;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("threeDSecure")]
    public bool ThreeDSecure { get; set; } = default!;

    [JsonPropertyName("card_threeDSecure")]
    public bool CardThreeDSecure { get; set; } = default!;

    [JsonPropertyName("save_card")]
    public bool SaveCard { get; set; } = default!;

    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; set; } = default!;

    [JsonPropertyName("product")]
    public string Product { get; set; } = default!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    [JsonPropertyName("transaction")]
    public Transaction Transaction { get; set; } = default!;

    [JsonPropertyName("reference")]
    public Reference Reference { get; set; } = default!;

    [JsonPropertyName("response")]
    public Response Response { get; set; } = default!;

    [JsonPropertyName("card_security")]
    public CardSecurity CardSecurity { get; set; } = default!;

    [JsonPropertyName("security")]
    public Security Security { get; set; } = default!;

    [JsonPropertyName("gateway")]
    public Gateway Gateway { get; set; } = default!;

    [JsonPropertyName("card")]
    public Card Card { get; set; } = default!;

    [JsonPropertyName("receipt")]
    public Receipt Receipt { get; set; } = default!;

    [JsonPropertyName("customer")]
    public Customer Customer { get; set; } = default!;

    [JsonPropertyName("merchant")]
    public Merchant Merchant { get; set; } = default!;

    [JsonPropertyName("source")]
    public Source Source { get; set; } = default!;

    [JsonPropertyName("redirect")]
    public Redirect Redirect { get; set; } = default!;

    [JsonPropertyName("post")]
    public Post Post { get; set; } = default!;

    [JsonPropertyName("authentication")]
    public Authentication Authentication { get; set; } = default!;

    [JsonPropertyName("activities")]
    public List<Activities> Activities { get; set; } = default!;

    [JsonPropertyName("auto_reversed")]
    public bool AutoReversed { get; set; } = default!;

    [JsonPropertyName("intent")]
    public Intent Intent { get; set; } = default!;

    [JsonPropertyName("protect")]
    public Protect Protect { get; set; } = default!;

    [JsonPropertyName("initiator")]
    public string Initiator { get; set; } = default!;
}

public class Expiry
{
    [JsonPropertyName("period")]
    public int Period { get; set; } = default!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;
}

public class Date
{
    [JsonPropertyName("created")]
    public long Created { get; set; } = default!;

    [JsonPropertyName("completed")]
    public long Completed { get; set; } = default!;

    [JsonPropertyName("transaction")]
    public long Transaction { get; set; } = default!;
}

public class Transaction
{
    [JsonPropertyName("authorization_id")]
    public string AuthorizationId { get; set; } = default!;

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = default!;

    [JsonPropertyName("created")]
    public string Created { get; set; } = default!;

    [JsonPropertyName("expiry")]
    public Expiry Expiry { get; set; } = default!;

    [JsonPropertyName("asynchronous")]
    public bool Asynchronous { get; set; } = default!;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; } = default!;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("date")]
    public Date Date { get; set; } = default!;
}

public class Reference
{
    [JsonPropertyName("track")]
    public string Track { get; set; } = default!;

    [JsonPropertyName("payment")]
    public string Payment { get; set; } = default!;

    [JsonPropertyName("acquirer")]
    public string Acquirer { get; set; } = default!;

    [JsonPropertyName("gateway")]
    public string Gateway { get; set; } = default!;
}

public class Response
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;
}

public class CardSecurity
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;
}

public class ThreeDSecure
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
}

public class Security
{
    [JsonPropertyName("threeDSecure")]
    public ThreeDSecure ThreeDSecure { get; set; } = default!;
}

public class Gateway
{
    [JsonPropertyName("response")]
    public Response Response { get; set; } = default!;
}

public class Card
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("first_six")]
    public string FirstSix { get; set; } = default!;

    [JsonPropertyName("first_eight")]
    public string FirstEight { get; set; } = default!;

    [JsonPropertyName("scheme")]
    public string Scheme { get; set; } = default!;

    [JsonPropertyName("brand")]
    public string Brand { get; set; } = default!;

    [JsonPropertyName("last_four")]
    public string LastFour { get; set; } = default!;
}

public class Receipt
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("email")]
    public bool Email { get; set; } = default!;

    [JsonPropertyName("sms")]
    public bool Sms { get; set; } = default!;
}

public class Customer
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = default!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;
}

public class Merchant
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = default!;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;
}

public class Source
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("payment_type")]
    public string PaymentType { get; set; } = default!;

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = default!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("on_file")]
    public bool OnFile { get; set; } = default!;

    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = default!;
}

public class Redirect
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;
}

public class Post
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;
}

public class Authentication
{
    [JsonPropertyName("acsEci")]
    public string AcsEci { get; set; } = default!;

    [JsonPropertyName("transaction_status")]
    public string TransactionStatus { get; set; } = default!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;
}

public class Activities
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("created")]
    public long Created { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; } = default!;

    [JsonPropertyName("remarks")]
    public string Remarks { get; set; } = default!;

    [JsonPropertyName("txn_id")]
    public string TxnId { get; set; } = default!;
}

public class Intent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;
}

public class Protect
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("is_in_exclusion_list")]
    public bool IsInExclusionList { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
}
