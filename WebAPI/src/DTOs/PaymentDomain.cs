namespace WebAPI.DTOs;

public sealed record PaymentRequest(
    string Token,
    decimal Amount,
    string Currency,
    // Guid UserId,
    string FirstName,
    string Email,
    string Provider = "TAP"
);

public sealed class PaymentResult
{
    public PaymentResultStatus Status { get; set; }

    public string? TransactionId { get; set; }

    public string? RedirectUrl { get; set; }

    public string? Error { get; set; }

    public string? Provider { get; set; }
}

public enum PaymentResultStatus
{
    Success = 0,
    RequiresAction = 1,
    Failed = 2
}

