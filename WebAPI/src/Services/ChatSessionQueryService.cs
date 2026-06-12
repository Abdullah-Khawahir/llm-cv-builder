namespace WebAPI.Services;

public interface IChatSessionQueryService
{
    Task<IList<ChatSessionListItemDto>> GetAllAsync();
    Task<ChatSessionDetailsDto?> GetByIdAsync(Guid id);
    Task<ChatSession?> GetEntityAsync(Guid id);
}

public sealed class ChatSessionQueryService(AppDbContext db) : IChatSessionQueryService
{
    private readonly AppDbContext _db = db;

    public async Task<ChatSessionDetailsDto?> GetByIdAsync(Guid id) =>
        await _db.ChatSessions
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => ChatSessionMapper.ToDTO(x))
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

    public async Task<ChatSession?> GetEntityAsync(Guid id) =>
        await _db.ChatSessions
        .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null)
        .ConfigureAwait(false);

    public async Task<IList<ChatSessionListItemDto>> GetAllAsync() =>
        await _db.ChatSessions
            .Where(x => x.DeletedAt == null)
            .AsNoTracking()
            .Select(x => ChatSessionMapper.ToListItemDTO(x))
            .ToListAsync()
            .ConfigureAwait(false);
}

