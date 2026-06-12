namespace WebAPI.Services;

public interface IChatSessionCommandService
{
    Task<ChatSessionDetailsDto> CreateAsync();
    Task DeleteAsync(Guid id);
    Task SaveHistoryAsync(Guid sessionId, ChatHistory history);
    Task UpdateHtmlAsync(Guid sessionId, string html);
    Task UpdateTitleAsync(Guid sessionId, string title);
}

public sealed class ChatSessionCommandService(AppDbContext db) : IChatSessionCommandService
{
    private readonly AppDbContext _db = db;

    public async Task<ChatSessionDetailsDto> CreateAsync()
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            ChatHistoryJson = "[]",
            CreatedAt = DateTime.UtcNow
        };

        _db.ChatSessions.Add(session);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ChatSessionMapper.ToDTO(session);
    }

    public async Task UpdateTitleAsync(Guid sessionId, string title)
    {
        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId).ConfigureAwait(false)
            ?? throw AppException.NotFound();

        session.Title = title;
        session.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateHtmlAsync(Guid sessionId, string html)
    {
        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId).ConfigureAwait(false)
            ?? throw AppException.NotFound();

        session.HtmlDocument = html;
        session.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task SaveHistoryAsync(Guid sessionId, ChatHistory history)
    {

        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId).ConfigureAwait(false)
            ?? throw AppException.NotFound();
        session.ChatHistory = history;
        session.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id)
    {
        var session = await _db.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == id)
            .ConfigureAwait(false) ?? throw AppException.NotFound($"session of id {id} is not found");

        if (session.DeletedAt is not null) return;


        session.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync()
            .ConfigureAwait(false);
    }
}
