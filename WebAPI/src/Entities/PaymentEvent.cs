using System.Text.Json.Serialization;

namespace WebAPI.Entities;

public sealed class PaymentEvent
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentEventType Type { get; set; }

    public string? Message { get; set; }

    public string? RawPayload { get; set; }

    public DateTime CreatedAt { get; set; }

    // public Payment Payment { get; set; } = default!;
}

public enum PaymentEventType
{
    Created,
    ChargeInitiated,
    ChargeSucceeded,
    ChargeFailed,
    WebhookReceived,
    WebhookVerified,
    RetryAttempted,
    RedirectRequired
}

