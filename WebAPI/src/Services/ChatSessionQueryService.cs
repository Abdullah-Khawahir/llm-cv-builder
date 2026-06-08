namespace WebAPI.Services;

public interface IChatSessionQueryService
{
    Task<IList<ChatSessionListItemDto>> GetAllAsync();
    Task<ChatSessionDetailsDto?> GetByIdAsync(Guid id);
    Task<ChatSession?> GetEntityAsync(Guid id);
}

public sealed class ChatSessionQueryService(ApplicationDbContext db) : IChatSessionQueryService
{
    public async Task<ChatSessionDetailsDto?> GetByIdAsync(Guid id) =>
        await db.ChatSessions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ChatSessionMapper.ToDTO(x))
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

    public async Task<ChatSession?> GetEntityAsync(Guid id) =>
        await db.ChatSessions
        .FirstOrDefaultAsync(x => x.Id == id)
        .ConfigureAwait(false);

    public async Task<IList<ChatSessionListItemDto>> GetAllAsync() =>
        await db.ChatSessions
            .AsNoTracking()
            .Select(x => ChatSessionMapper.ToListItemDTO(x))
            .ToListAsync()
            .ConfigureAwait(false);
}

