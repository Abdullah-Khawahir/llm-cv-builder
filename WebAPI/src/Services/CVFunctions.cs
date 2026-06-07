namespace WebAPI.Services;

public sealed class CVFunctions(
    IChatSessionService sessions,
    Guid sessionId,
    ILogger<CVFunctions> log)
{
    private readonly IChatSessionService _sessions = sessions;
    private readonly Guid _sessionId = sessionId;
    private readonly ILogger<CVFunctions> _log = log;

    [KernelFunction("WriteCV")]
    [Description("Writes and saves the generated CV HTML.")]
    public async Task<bool> WriteCVAsync(string html)
    {
        _log.LogInformation("WriteCV invoked for {SessionId}", _sessionId);

        await _sessions.UpdateHtmlAsync(_sessionId, html).ConfigureAwait(false);

        return true;
    }

    [KernelFunction("GetCV")]
    [Description("Returns the current saved CV HTML.")]
    public async Task<string> GetCurrentCVAsync()
    {
        _log.LogInformation("GetCV invoked for {SessionId}", _sessionId);

        var session = await _sessions.GetByIdAsync(_sessionId).ConfigureAwait(false);
        return session?.HtmlDocument ?? string.Empty;
    }
}
