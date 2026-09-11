namespace WebAPI.Entities;

public sealed class Payment
{
    public Guid Id { get; set; }
    // public Guid UserId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";

    public string Provider { get; set; } = "Tap";
    public string? ProviderRef { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<PaymentEvent> Events { get; set; } = default!;
    // public User User { get; set; } = default!;
}
