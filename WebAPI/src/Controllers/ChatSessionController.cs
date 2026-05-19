namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatSessionController : ControllerBase
{
    private readonly IChatSessionService _service;

    public ChatSessionController(IChatSessionService service)
    {
        _service = service;
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<ChatSession>>> GetUserHistoryAsync()
    {
        var sessions = await _service.GetAllSessionsAsync();
        return Ok(sessions);
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<ChatSession>> GetChatSessionByIdAsync([FromRoute] Guid id)
    {
        var chat = await _service.GetByIdAsync(id);
        if (chat is null)
        {
            return NotFound();
        }
        return chat;
    }
    [HttpPost("chat/{id}/stream")]
    public async Task<IAsyncEnumerable<string>> StreamChat(Guid id, [FromBody] string prompt)
    {
        var stream = _service.ProcessPromptStreamingAsync(id, prompt);

        return stream;
    }

    [HttpPost("{id:Guid}/chat")]
    public async Task<ActionResult<ChatSession>> ChatPromptAsync([FromRoute] Guid id, [FromBody] string prompt)
    {
        var session = await _service.ProcessPromptAsync(id, prompt);

        if (session is null)
        {
            return NotFound();
        }
        return session;
    }

    [HttpPost("new")]
    public async Task<ActionResult<List<ChatSession>>> CreateNewAsync()
    {
        var session = await _service.CreateNewAsync();
        return CreatedAtAction(nameof(GetChatSessionByIdAsync), new { id = session.Id }, session);
    }

}



