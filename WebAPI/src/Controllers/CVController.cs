using System.Security.Cryptography;
using Minio.DataModel.Args;
using Minio.Exceptions;
using WebAPI.Services.Fonts;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/cv")]
public sealed class CVController(AppDbContext db, IMinioClient minio, IFontCatalog fonts, ILogger<CVController> logger) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IMinioClient _minio = minio;
    private readonly IFontCatalog _fonts = fonts;
    private readonly ILogger<CVController> _logger = logger;

    [HttpGet("preview/{id:guid}")]
    public async Task<IResult> PreviewCV([FromRoute] Guid id, [FromQuery] string? font = null)
    {
        _logger.LogInformation("Processing CV preview request for session {SessionId}", id);
        var session = await _db.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound($"Session not found: {id}");

        var fontId = string.IsNullOrWhiteSpace(font) ? session.FontFamilyId : _fonts.NormalizeId(font);
        var fontDef = _fonts.Get(fontId);
        var effectiveFontId = fontDef?.Id ?? session.FontFamilyId;
        var htmlHash = ComputeHash(session.HtmlDocument, effectiveFontId);
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
                memory = await PdfGenerator.GenerateAsync(session.HtmlDocument, fontDef);

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



    private static string ComputeHash(string html, string fontId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(html.Trim() + "|" + fontId));
        return Convert.ToHexString(bytes)[..16];
    }
}
