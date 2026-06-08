// namespace WebAPI.Services;
//
// using Microsoft.SemanticKernel.ChatCompletion;
//
//
// public interface IChatSessionService
// {
//     Task<ChatSessionDetailsDto?> GetByIdAsync(Guid id);
//     Task<ChatSessionDetailsDto> CreateAsync();
//     Task<ChatSession?> GetEntityAsync(Guid id);
//
//     Task UpdateHtmlAsync(Guid id, string html);
//     Task UpdateSessionTitleAsync(Guid id, string newTitle);
//
//     Task SaveHistoryAsync(Guid id, ChatHistory history);
//
//     IAsyncEnumerable<ChatStreamEvent> StreamAsync(
//         Guid sessionId,
//         string prompt,
//         CancellationToken cancellationToken = default);
//
//     Task<IEnumerable<ChatSessionListItemDto>> GetAllAsync();
// }
