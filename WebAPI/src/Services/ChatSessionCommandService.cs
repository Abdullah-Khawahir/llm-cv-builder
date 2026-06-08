namespace WebAPI.Services;

public interface IChatSessionCommandService
{
    Task<ChatSessionDetailsDto> CreateAsync();
    Task SaveHistoryAsync(Guid sessionId, ChatHistory history);
    Task UpdateHtmlAsync(Guid sessionId, string html);
    Task UpdateTitleAsync(Guid sessionId, string title);
}

public sealed class ChatSessionCommandService(ApplicationDbContext db) : IChatSessionCommandService
{
    public async Task<ChatSessionDetailsDto> CreateAsync()
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            ChatHistoryJson = "[]",
            CreatedAt = DateTime.UtcNow
        };

        db.ChatSessions.Add(session);
        await db.SaveChangesAsync().ConfigureAwait(false);

        return ChatSessionMapper.ToDTO(session);
    }

    public async Task UpdateTitleAsync(Guid sessionId, string title)
    {
        var session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId).ConfigureAwait(false)
            ?? throw AppException.NotFound();

        session.Title = title;
        session.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateHtmlAsync(Guid sessionId, string html)
    {
        var session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId).ConfigureAwait(false)
            ?? throw AppException.NotFound();

        session.HtmlDocument = html;
        session.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task SaveHistoryAsync(Guid sessionId, ChatHistory history)
    {

        var session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId).ConfigureAwait(false)
            ?? throw AppException.NotFound();
        session.ChatHistory = history;
        session.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync().ConfigureAwait(false);
    }
}
