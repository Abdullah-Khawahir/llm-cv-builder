using WebAPI.Services.Fonts;

namespace WebAPI.Services;

public sealed class CVFunctions(
    IChatSessionCommandService command,
    IChatSessionQueryService query,
    IFontCatalog fonts,
    Guid sessionId,
    ILogger<CVFunctions> log)
{
    private readonly IChatSessionCommandService _command = command;
    private readonly IChatSessionQueryService _query = query;
    private readonly IFontCatalog _fonts = fonts;
    private readonly Guid _sessionId = sessionId;
    private readonly ILogger<CVFunctions> _log = log;

    [KernelFunction("WriteCV")]
    [Description("Writes and saves the generated CV HTML.")]
    public async Task<bool> WriteCVAsync(string html)
    {
        _log.LogInformation("WriteCV invoked for {SessionId}", _sessionId);

        await _command.UpdateHtmlAsync(_sessionId, html).ConfigureAwait(false);

        return true;
    }

    [KernelFunction("GetCV")]
    [Description("Returns the current saved CV HTML.")]
    public async Task<string> GetCurrentCVAsync()
    {
        _log.LogInformation("GetCV invoked for {SessionId}", _sessionId);

        var session = await _query.GetByIdAsync(_sessionId).ConfigureAwait(false);
        return session?.HtmlDocument ?? string.Empty;
    }

    [KernelFunction("ListFonts")]
    [Description("Lists available CV fonts with ids and Arabic-support flags. Call before SetFont if unsure which font to use.")]
    public Task<string> ListFontsAsync()
    {
        var lines = _fonts.List()
            .Select(f => $"- {f.Id}: {f.DisplayName} (Arabic: {(f.SupportsArabic ? "yes" : "no")})");
        return Task.FromResult(string.Join("\n", lines));
    }

    [KernelFunction("SetFont")]
    [Description("Changes the CV font. fontId must be an id from ListFonts (e.g. inter, cairo, ibm-plex-sans-arabic). Call this when the user names a font. Never invent @font-face URLs yourself.")]
    public async Task<string> SetFontAsync(
        [Description("Font id, case-insensitive, e.g. 'cairo'. Fuzzy names like 'Cairo font' are normalized.")] string fontId)
    {
        var normalized = _fonts.NormalizeId(fontId);
        var font = _fonts.Get(normalized);
        if (font is null || !string.Equals(font.Id, normalized, StringComparison.OrdinalIgnoreCase))
        {
            var available = string.Join(", ", _fonts.List().Select(f => f.Id));
            return $"Unknown font '{fontId}'. Available: {available}. Ask the user to pick one.";
        }

        _log.LogInformation("SetFont {Font} for {SessionId}", font.Id, _sessionId);
        await _command.UpdateFontAsync(_sessionId, font.Id).ConfigureAwait(false);
        return $"Font set to {font.DisplayName} ({font.Family}). It applies on the next preview. Arabic supported: {(font.SupportsArabic ? "yes" : "no")}.";
    }


    [KernelFunction("SetSessionTitle")]
    [Description("functions sets the session tite. this title is shown to the user")]
    public async Task<bool> SetSessionTitle(string newTitle)
    {
        try
        {
            await _command.UpdateTitleAsync(_sessionId, newTitle).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
