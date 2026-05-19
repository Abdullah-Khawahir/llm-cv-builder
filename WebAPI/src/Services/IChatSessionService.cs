namespace WebAPI.Services;

public interface IChatSessionService
{
    Task<ChatSession> CreateNewAsync();
    Task<ICollection<ChatSession>> GetAllSessionsAsync();
    Task<ChatSession?> GetByIdAsync(Guid id);
    Task<ChatSession?> GetSessionForPromptAsync(Guid id);
    Task<ChatSession?> ProcessPromptAsync(Guid id, string prompt);
    IAsyncEnumerable<string> ProcessPromptStreamingAsync(Guid id, string prompt);
}

