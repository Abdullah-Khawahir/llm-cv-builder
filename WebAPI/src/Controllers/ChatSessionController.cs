namespace WebAPI.Controllers;

[ApiController]
[Route("api/chat-sessions")]
public sealed class ChatSessionController(IChatSessionService sessions) : ControllerBase
{
    private readonly IChatSessionService _sessions = sessions;

    [HttpGet]
    public async Task<ActionResult<ChatSessionDto[]>> GetAllAsync()
    {
        var sessions = await _sessions.GetAllAsync() ?? [];
        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChatSessionDto>> GetById(Guid id)
    {
        var session = await _sessions.GetByIdAsync(id);

        if (session is null)
        {
            return NotFound();
        }

        return Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult<ChatSessionDto>> CreateAsync()
    {
        var session = await _sessions.CreateAsync();

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
    [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
    public async Task StreamAsync(
        Guid id,
        [FromBody] ChatPromptRequest request,
        CancellationToken cancellationToken)
    {
        Response.StatusCode = 200;
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        await foreach (var evt in _sessions.StreamAsync(id, request.Prompt, cancellationToken))
        {
            var json = JsonSerializer.Serialize(evt);

            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);

            await Response.Body.FlushAsync(cancellationToken);
        }

    }
}
