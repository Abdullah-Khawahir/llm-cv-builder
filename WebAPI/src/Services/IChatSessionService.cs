namespace WebAPI.Services;

using Microsoft.SemanticKernel.ChatCompletion;


public interface IChatSessionService
{
    Task<ChatSessionDto?> GetByIdAsync(Guid id);
    Task<ChatSessionDto> CreateAsync();
    Task<ChatSession?> GetEntityAsync(Guid id);

    Task UpdateHtmlAsync(Guid id, string html);

    Task SaveHistoryAsync(Guid id, ChatHistory history);

    IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        Guid sessionId,
        string prompt,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ChatSessionDto>> GetAllAsync();
}
