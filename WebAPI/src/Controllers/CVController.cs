using System.Security.Cryptography;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/cv")]
public sealed class CVController(AppDbContext db, IMinioClient minio, ILogger<CVController> logger) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IMinioClient _minio = minio;
    private readonly ILogger<CVController> _logger = logger;

    [HttpGet("preview/{id:guid}")]
    public async Task<IResult> PreviewCV([FromRoute] Guid id)
    {
        _logger.LogInformation("Processing CV preview request for session {SessionId}", id);
        var session = await _db.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound($"Session not found: {id}");

        var htmlHash = ComputeHash(session.HtmlDocument);
        var fileName = $"{session.Id}_{htmlHash}.pdf";

        var memory = new MemoryStream();
        var exists = true;

        try
        {
            _logger.LogDebug("Checking Minio for existing PDF: {FileName}", fileName);
            await _minio.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket("pdfs")
                    .WithObject(fileName)
                    .WithCallbackStream(s => s.CopyTo(memory)));

            memory.Position = 0;
            _logger.LogInformation("Cache hit for PDF: {FileName}", fileName);
        }
        catch (ObjectNotFoundException)
        {
            _logger.LogInformation("Cache miss for PDF: {FileName}. Generating new PDF...", fileName);
            exists = false;
        }

        if (!exists)
        {
            try
            {
                memory = await PdfGenerator.GenerateAsync(session.HtmlDocument);

                await _minio.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket("pdfs")
                        .WithObject(fileName)
                        .WithStreamData(memory)
                        .WithObjectSize(memory.Length)
                        .WithContentType("application/pdf"));

                memory.Position = 0;
                _logger.LogInformation("Successfully generated and cached PDF: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate or cache PDF for session {SessionId}", id);
                throw;
            }
        }

        return Results.File(memory, "application/pdf");
    }



    private static string ComputeHash(string html)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(html.Trim()));
        return Convert.ToHexString(bytes)[..16];
    }
}
