namespace WebAPI.Services;

public sealed class CVFunctions(
    IChatSessionCommandService command,
    IChatSessionQueryService query,
    Guid sessionId,
    ILogger<CVFunctions> log)
{
    private readonly IChatSessionCommandService _command = command;
    private readonly IChatSessionQueryService _query = query;
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
