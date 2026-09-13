using WebAPI.Services.Fonts;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/fonts")]
public sealed class FontsController(IFontCatalog catalog, ILogger<FontsController> logger) : ControllerBase
{
    private readonly IFontCatalog _catalog = catalog;
    private readonly ILogger<FontsController> _logger = logger;

    public sealed record FontDto(string Id, string DisplayName, string Family, string Source, bool SupportsArabic, string CssStack);

    [HttpGet]
    public ActionResult<FontDto[]> List() =>
        Ok(_catalog.List()
            .Select(f => new FontDto(f.Id, f.DisplayName, f.Family, f.Source, f.SupportsArabic, f.CssStack))
            .ToArray());

    [HttpGet("{id}")]
    public ActionResult<FontDto> GetById(string id)
    {
        var normalized = _catalog.NormalizeId(id);
        var font = _catalog.List().FirstOrDefault(f => string.Equals(f.Id, normalized, StringComparison.Ordinal));
        if (font is null) return NotFound();
        return Ok(new FontDto(font.Id, font.DisplayName, font.Family, font.Source, font.SupportsArabic, font.CssStack));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<FontDto[]>> Refresh(CancellationToken ct)
    {
        await _catalog.RefreshAsync(ct).ConfigureAwait(false);
        return List();
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<FontDto>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest("No file uploaded.");
        _logger.LogInformation("Uploading font {Name} ({Length} bytes)", file.FileName, file.Length);
        await using var stream = file.OpenReadStream();
        var saved = await _catalog.SaveUploadAsync(file.FileName, stream, ct).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetById), new { id = saved.Id },
            new FontDto(saved.Id, saved.DisplayName, saved.Family, saved.Source, saved.SupportsArabic, saved.CssStack));
    }
}
