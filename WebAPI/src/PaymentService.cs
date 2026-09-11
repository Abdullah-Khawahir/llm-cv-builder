namespace WebAPI.Services;

public sealed class PaymentService
{
    private readonly IPaymentProvider _provider;
    private readonly AppDbContext _db;
    private readonly ILogger<PaymentService> _log;

    public PaymentService(IPaymentProvider provider, AppDbContext db, ILogger<PaymentService> log)
    {
        _provider = provider;
        _db = db;
        _log = log;
    }

    public async Task<PaymentResult> PayAsync(PaymentRequest request, CancellationToken ct)
    {
        var payment = CreatePaymentRecord(request);

        try
        {
            await PersistPaymentAsync(payment, "Payment created.", PaymentEventType.Created, ct).ConfigureAwait(false);
            await PersistPaymentAsync(payment, "Charge initiated.", PaymentEventType.ChargeInitiated, ct).ConfigureAwait(false);

            _log.LogInformation("Calling provider. PaymentId: {PaymentId}, Provider: {Provider}", payment.Id, request.Provider);
            var result = await _provider.ChargeAsync(payment.Id, request, ct).ConfigureAwait(false);
            _log.LogInformation("Provider response. PaymentId: {PaymentId}, Status: {Status}", payment.Id, result.Status);

            return await HandleProviderResult(payment, result, request, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return await HandleProviderException(payment, ex, request, ct).ConfigureAwait(false);
        }
    }

    private Payment CreatePaymentRecord(PaymentRequest request)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Currency = request.Currency,
            Provider = request.Provider,
            CreatedAt = DateTime.UtcNow
        };
        _db.Payments.Add(payment);
        return payment;
    }

    private async Task PersistPaymentAsync(Payment payment, string message, PaymentEventType type, CancellationToken ct)
    {
        _db.PaymentEvents.Add(new PaymentEvent
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            Message = message
        });
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task<PaymentResult> HandleProviderResult(Payment payment, PaymentResult result, PaymentRequest request, CancellationToken ct)
    {
        payment.ProviderRef = result.TransactionId;

        return result.Status switch
        {
            PaymentResultStatus.Success => await CompletePayment(payment, result, request, ct).ConfigureAwait(false),
            PaymentResultStatus.RequiresAction => await RequireAction(payment, result, request, ct).ConfigureAwait(false),
            PaymentResultStatus.Failed => await FailPayment(payment, result.Error ?? "", request, ct).ConfigureAwait(false),
            _ => await FailPayment(payment, $"Unexpected status: {result.Status}", request, ct)
.ConfigureAwait(false)
        };
    }

    private async Task<PaymentResult> CompletePayment(Payment p, PaymentResult r, PaymentRequest req, CancellationToken ct)
    {
        await PersistPaymentAsync(p, $"Captured. Tx: {r.TransactionId}", PaymentEventType.ChargeSucceeded, ct).ConfigureAwait(false);
        _log.LogInformation("Payment succeeded. PaymentId: {PaymentId}", p.Id);
        return new PaymentResult { Status = PaymentResultStatus.Success, TransactionId = r.TransactionId, Provider = req.Provider };
    }

    private async Task<PaymentResult> RequireAction(Payment p, PaymentResult r, PaymentRequest req, CancellationToken ct)
    {
        await PersistPaymentAsync(p, $"3DS required. Redirect: {r.RedirectUrl}", PaymentEventType.RedirectRequired, ct).ConfigureAwait(false);
        return new PaymentResult { Status = PaymentResultStatus.RequiresAction, RedirectUrl = r.RedirectUrl, TransactionId = r.TransactionId, Provider = req.Provider };
    }

    private async Task<PaymentResult> FailPayment(Payment p, string error, PaymentRequest req, CancellationToken ct)
    {
        p.FailureReason = error;
        await PersistPaymentAsync(p, $"Failed: {error}", PaymentEventType.ChargeFailed, ct).ConfigureAwait(false);
        _log.LogWarning("Payment failed. PaymentId: {PaymentId}, Reason: {Reason}", p.Id, error);
        return new PaymentResult { Status = PaymentResultStatus.Failed, Error = error, TransactionId = p.ProviderRef, Provider = req.Provider };
    }

    private async Task<PaymentResult> HandleProviderException(Payment p, Exception ex, PaymentRequest req, CancellationToken ct)
    {
        _log.LogError(ex, "Provider exception. PaymentId: {PaymentId}", p.Id);

        await FailPayment(p, ex.Message, req, ct).ConfigureAwait(false);
        return new PaymentResult { Status = PaymentResultStatus.Failed, Error = ex.Message };
    }
}
