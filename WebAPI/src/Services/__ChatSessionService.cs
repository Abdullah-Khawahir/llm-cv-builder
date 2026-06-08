// using System.Runtime.CompilerServices;
// using Microsoft.SemanticKernel.Connectors.Google;
//
// namespace WebAPI.Services;
//
//
// public sealed class ChatSessionService(
//     ApplicationDbContext db,
//     ILogger<ChatSessionService> log,
//     ILoggerFactory loggerFactory
//     ) : IChatSessionService
// {
//     private readonly ApplicationDbContext _db = db;
//     private readonly ILogger<ChatSessionService> _log = log;
//     private readonly ILoggerFactory _loggerFactory = loggerFactory;
//
//     public async Task<ChatSessionDetailsDto?> GetByIdAsync(Guid id)
//     {
//         _log.LogDebug("Fetching chat session DTO for {SessionId}", id);
//         return await _db.ChatSessions
//             .AsNoTracking()
//             .Where(x => x.Id == id)
//             .Select(x => ChatSessionMapper.ToDTO(x))
//             .FirstOrDefaultAsync().ConfigureAwait(false);
//     }
//
//     public async Task<ChatSession?> GetEntityAsync(Guid id)
//     {
//         _log.LogDebug("Fetching chat session entity for {SessionId}", id);
//         return await _db.ChatSessions
//             .FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
//     }
//
//     public async Task<ChatSessionDetailsDto> CreateAsync()
//     {
//         _log.LogInformation("Creating a new chat session");
//         var session = new ChatSession
//         {
//             Id = Guid.NewGuid(),
//             ChatHistoryJson = "[]",
//             CreatedAt = DateTime.UtcNow
//         };
//
//         _db.ChatSessions.Add(session);
//
//         await _db.SaveChangesAsync().ConfigureAwait(false);
//
//         _log.LogInformation("Created chat session {SessionId}", session.Id);
//         return ChatSessionMapper.ToDTO(session);
//     }
//
//     public async Task UpdateHtmlAsync(Guid id, string html)
//     {
//         _log.LogInformation("Updating HTML for session {SessionId}", id);
//         var session = await GetEntityAsync(id).ConfigureAwait(false) ?? throw new InvalidOperationException("Session not found.");
//         session.UpdatedAt = DateTime.UtcNow;
//         session.HtmlDocument = html;
//
//         _db.Entry(session).CurrentValues.SetValues(session);
//
//         await _db.SaveChangesAsync().ConfigureAwait(false);
//         _log.LogDebug("Successfully updated HTML for session {SessionId}", id);
//     }
//
//     public async Task SaveHistoryAsync(Guid id, ChatHistory history)
//     {
//         _log.LogInformation("Saving chat history for session {SessionId}", id);
//         var session = await GetEntityAsync(id).ConfigureAwait(false) ?? throw new InvalidOperationException("Session not found.");
//
//         session.UpdatedAt = DateTime.UtcNow;
//         session.ChatHistory = history;
//
//         _db.Entry(session).CurrentValues.SetValues(session);
//
//         await _db.SaveChangesAsync().ConfigureAwait(false);
//         _log.LogDebug("Successfully saved history for session {SessionId}", id);
//     }
//
//     public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
//         Guid sessionId,
//         string prompt,
//         [EnumeratorCancellation] CancellationToken cancellationToken = default)
//     {
//         _log.LogInformation("Initializing stream for session {SessionId}", sessionId);
//         var session = await GetEntityAsync(sessionId).ConfigureAwait(false);
//         if (session is null)
//         {
//             _log.LogWarning("Stream failed: Session {SessionId} not found", sessionId);
//             yield return ChatStreamEvent.CreateErrorEvent("Session not found.");
//             yield break;
//         }
//
//         var history = session.ChatHistory;
//         EnsureSystemPrompt(history);
//         history.AddUserMessage(prompt);
//
//         _log.LogDebug("Creating kernel for session {SessionId}", sessionId);
//         var kernel = CreateKernel(sessionId);
//         var chatService = kernel.GetRequiredService<IChatCompletionService>();
//
//         var settings = new GeminiPromptExecutionSettings
//         {
//             FunctionChoiceBehavior = FunctionChoiceBehavior.Required(),
//             ToolCallBehavior = GeminiToolCallBehavior.EnableKernelFunctions,
//         };
//
//         yield return ChatStreamEvent.CreateThinkingEvent();
//
//         IAsyncEnumerable<StreamingChatMessageContent>? chunks = null;
//         Exception? setupError = null;
//
//         // 1. Catch initialization errors (No yield inside this try-catch)
//         try
//         {
//             _log.LogDebug("Requesting streaming contents from provider for session {SessionId}", sessionId);
//             chunks = chatService.GetStreamingChatMessageContentsAsync(history, settings, kernel, cancellationToken);
//         }
//         catch (Exception ex)
//         {
//             _log.LogError(ex, "Failed to initialize stream for session {SessionId}", sessionId);
//             setupError = ex;
//         }
//
//         if (setupError != null)
//         {
//             yield return ChatStreamEvent.CreateErrorEvent($"Failed to initialize stream: {setupError.Message}");
//             yield break;
//         }
//
//         if (chunks is null)
//         {
//             _log.LogError("Stream chunks were null for session {SessionId}", sessionId);
//             yield return ChatStreamEvent.CreateErrorEvent($"failed to initialize service provider streams");
//             yield break;
//         }
//
//         // 2. Manual enumeration to safely catch streaming errors
//         var enumerator = chunks.GetAsyncEnumerator(cancellationToken);
//         var assistantMessage = new StringBuilder();
//         Exception? streamError = null;
//
//         try // Outer try-finally is ALLOWED to contain yield return
//         {
//             while (true)
//             {
//                 bool hasMore;
//                 try // Inner try-catch has NO yield return, so it is ALLOWED
//                 {
//                     hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);
//                 }
//                 catch (Exception ex)
//                 {
//                     _log.LogError(ex, "Error occurred while enumerating stream for session {SessionId}", sessionId);
//                     streamError = ex;
//                     break; // Safely exit the loop
//                 }
//
//                 if (!hasMore) break;
//
//                 var chunk = enumerator.Current;
//                 if (string.IsNullOrWhiteSpace(chunk.Content))
//                 {
//                     continue;
//                 }
//
//                 assistantMessage.Append(chunk.Content);
//                 yield return ChatStreamEvent.CreateTokenEvent(chunk.Content);
//             }
//         }
//         finally
//         {
//             // Ensures resources are cleaned up even if an error occurs
//             await enumerator.DisposeAsync().ConfigureAwait(false);
//         }
//
//         // 3. Yield the error event OUTSIDE the try-catch block (Compiler allows this)
//         if (streamError != null)
//         {
//             yield return ChatStreamEvent.CreateErrorEvent($"Failed to get stream from provider: {streamError.Message}");
//             yield break;
//         }
//
//         // 4. Success path
//         _log.LogInformation("Stream completed successfully for session {SessionId}. Saving history.", sessionId);
//         history.AddAssistantMessage(assistantMessage.ToString());
//         await SaveHistoryAsync(sessionId, history).ConfigureAwait(false);
//
//         yield return ChatStreamEvent.CreateCompletedEvent();
//         yield return ChatStreamEvent.CreateSessionUpdateEvent(ChatSessionMapper.ToDTO(session));
//     }
//
//
//     private Kernel CreateKernel(Guid sessionId)
//     {
//         var builder = Kernel.CreateBuilder();
//
//         builder.Services.AddLogging(l =>
//         {
//             l.AddSerilog();
//             l.SetMinimumLevel(LogLevel.Trace);
//         });
//         var cvFunctionsLogger = _loggerFactory.CreateLogger<CVFunctions>();
//         builder.Plugins.AddFromObject(
//             new CVFunctions(this, sessionId, cvFunctionsLogger));
//
//         builder.AddOpenAIChatCompletion(
//             modelId: "google/gemini-2.5-flash-lite",
//             apiKey: Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"),
//             endpoint: new Uri("https://openrouter.ai/api/v1"),
//             httpClient: new HttpClient
//             {
//                 DefaultRequestHeaders =
//                 {
//                     { "HTTP-Referer", "http://localhost:5044" },
//                     { "X-Title", "CV-App" }
//                 }
//             });
//
//         return builder.Build();
//     }
//
//     private static void EnsureSystemPrompt(ChatHistory history)
//     {
//         if (history.Any(x => x.Role == AuthorRole.System))
//         {
//             return;
//         }
//         history.AddSystemMessage(Prompts.USER_INTERACTION_AGENT_SYS_PROMPT);
//     }
//
//     public async Task<IEnumerable<ChatSessionListItemDto>> GetAllAsync()
//     {
//         _log.LogInformation("Fetching all chat sessions");
//
//         var results = await _db.ChatSessions
//             .AsNoTracking()
//             .Select(e => ChatSessionMapper.ToListItemDTO(e))
//             .ToListAsync()
//             .ConfigureAwait(false);
//
//         _log.LogInformation("Successfully retrieved {Count} sessions", results.Count);
//         return results;
//     }
//
//     public async Task UpdateSessionTitleAsync(Guid id, string newTitle)
//     {
//         _log.LogInformation("Updating Title for session {SessionId}", id);
//         var session = await GetEntityAsync(id).ConfigureAwait(false) ?? throw new InvalidOperationException("Session not found.");
//
//         session.UpdatedAt = DateTime.UtcNow;
//         session.Title = newTitle;
//
//         _db.Entry(session).CurrentValues.SetValues(session);
//
//         await _db.SaveChangesAsync().ConfigureAwait(false);
//         _log.LogDebug("Successfully Title for session {SessionId} to {newTitle}", id, newTitle);
//     }
// }
//
//
