namespace WebAPI.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly PaymentService _service;
    private readonly AppDbContext _db;

    public PaymentsController(PaymentService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }
    [HttpGet]
    public async Task<IResult> getAllPayments()
    {
        return TypedResults.Ok(await _db.Payments.AsNoTracking()
            .Include(p => p.Events)
            .ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<
    Results<Ok<Payment>, NotFound>
        > GetPaymentById([FromRoute] Guid id)
    {
        return await _db.Payments
                    .AsNoTracking()
                    .Include(p => p.Events)
                    .FirstOrDefaultAsync(p => p.Id == id) is { } payment ?
                    TypedResults.Ok(payment)
                    : TypedResults.NotFound();
    }

    [HttpPost]
    public async Task<
        Results<
        Ok<PaymentResult>,
        BadRequest<PaymentResult>,
        InternalServerError<PaymentResult>
        >>
        Pay(PaymentRequest request, CancellationToken ct)
    {
        var result = await _service.PayAsync(request, ct);

        return result switch
        {
            { Status: PaymentResultStatus.Success } => TypedResults.Ok(result),
            { Status: PaymentResultStatus.RequiresAction, RedirectUrl: not null } => TypedResults.Ok(result),
            { Status: PaymentResultStatus.Failed } => TypedResults.BadRequest(result),
            _ => TypedResults.InternalServerError(result),
        };
    }
}
