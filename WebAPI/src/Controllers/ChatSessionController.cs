namespace WebAPI.Controllers;

[ApiController]
[Route("api/chat-sessions")]
public sealed class ChatSessionController(IChatSessionService sessions, ILogger<ChatSessionController> logger) : ControllerBase
{
    private readonly IChatSessionService _sessions = sessions;
    private readonly ILogger<ChatSessionController> _logger = logger;

    [HttpGet]
    public async Task<ActionResult<ChatSessionDto[]>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all chat sessions");
        var allSessions = await _sessions.GetAllAsync() ?? [];
        _logger.LogInformation("Successfully retrieved {Count} sessions", allSessions.Count());
        return Ok(allSessions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChatSessionDto>> GetById(Guid id)
    {
        _logger.LogInformation("Fetching chat session {SessionId}", id);
        var session = await _sessions.GetByIdAsync(id);

        if (session is null)
        {
            _logger.LogWarning("Chat session {SessionId} not found", id);
            return NotFound();
        }

        return Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult<ChatSessionDto>> CreateAsync()
    {
        _logger.LogInformation("Creating new chat session");
        var session = await _sessions.CreateAsync();
        _logger.LogInformation("Created chat session {SessionId}", session.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = session.Id },
            session);
    }

    [HttpPost("{id:guid}/stream")]
    [Produces("text/event-stream")]
    [ProducesResponseType(typeof(Token), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SessionUpdate), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Completed), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Thinking), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEvent), StatusCodes.Status500InternalServerError)]
    public async Task StreamAsync(
        Guid id,
        [FromBody] ChatPromptRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting stream for session {SessionId} with prompt: {Prompt}", id, request.Prompt);
        Response.StatusCode = 200;
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        try
        {
            await foreach (var evt in _sessions.StreamAsync(id, request.Prompt, cancellationToken))
            {
                var json = JsonSerializer.Serialize(evt);
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
            _logger.LogInformation("Completed stream for session {SessionId}", id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stream cancelled for session {SessionId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stream for session {SessionId}", id);
            throw;
        }
    }
}
