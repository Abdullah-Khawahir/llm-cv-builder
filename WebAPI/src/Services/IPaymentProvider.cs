namespace WebAPI.Services;

public interface IPaymentProvider
{
    Task<PaymentResult> ChargeAsync(Guid PaymentId, PaymentRequest request, CancellationToken ct);
}
