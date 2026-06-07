using System.Runtime.CompilerServices;

namespace WebAPI.Services;


public sealed class ChatSessionService(
    ApplicationDbContext db,
    ILogger<ChatSessionService> log,
    ILoggerFactory loggerFactory
    ) : IChatSessionService
{
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger<ChatSessionService> _log = log;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    public async Task<ChatSessionDto?> GetByIdAsync(Guid id)
    {
        _log.LogDebug("Fetching chat session DTO for {SessionId}", id);
        return await _db.ChatSessions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ChatSessionMapper.ToDTO(x))
            .FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<ChatSession?> GetEntityAsync(Guid id)
    {
        _log.LogDebug("Fetching chat session entity for {SessionId}", id);
        return await _db.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
    }

    public async Task<ChatSessionDto> CreateAsync()
    {
        _log.LogInformation("Creating a new chat session");
        var session = new ChatSession(Guid.NewGuid(), string.Empty, JsonSerializer.Serialize(new ChatHistory()), default);

        _db.ChatSessions.Add(session);

        await _db.SaveChangesAsync().ConfigureAwait(false);

        _log.LogInformation("Created chat session {SessionId}", session.Id);
        return ChatSessionMapper.ToDTO(session);
    }

    public async Task UpdateHtmlAsync(Guid id, string html)
    {
        _log.LogInformation("Updating HTML for session {SessionId}", id);
        var session = await GetEntityAsync(id).ConfigureAwait(false) ?? throw new InvalidOperationException("Session not found.");

        _db.Entry(session).CurrentValues.SetValues(session with { HtmlDocument = html });

        await _db.SaveChangesAsync().ConfigureAwait(false);
        _log.LogDebug("Successfully updated HTML for session {SessionId}", id);
    }

    public async Task SaveHistoryAsync(Guid id, ChatHistory history)
    {
        _log.LogInformation("Saving chat history for session {SessionId}", id);
        var session = await GetEntityAsync(id).ConfigureAwait(false) ?? throw new InvalidOperationException("Session not found.");

        _db.Entry(session)
            .CurrentValues
            .SetValues(
                session with
                {
                    ChatHistoryJson = JsonSerializer.Serialize(history)
                });

        await _db.SaveChangesAsync().ConfigureAwait(false);
        _log.LogDebug("Successfully saved history for session {SessionId}", id);
    }

    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        Guid sessionId,
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _log.LogInformation("Initializing stream for session {SessionId}", sessionId);
        var session = await GetEntityAsync(sessionId).ConfigureAwait(false);
        if (session is null)
        {
            _log.LogWarning("Stream failed: Session {SessionId} not found", sessionId);
            yield return ChatStreamEvent.CreateErrorEvent("Session not found.");
            yield break;
        }

        var history = session.ChatHistory;
        EnsureSystemPrompt(history);
        history.AddUserMessage(prompt);

        _log.LogDebug("Creating kernel for session {SessionId}", sessionId);
        var kernel = CreateKernel(sessionId);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        yield return ChatStreamEvent.CreateThinkingEvent();

        IAsyncEnumerable<StreamingChatMessageContent>? chunks = null;
        Exception? setupError = null;

        // 1. Catch initialization errors (No yield inside this try-catch)
        try
        {
            _log.LogDebug("Requesting streaming contents from provider for session {SessionId}", sessionId);
            chunks = chatService.GetStreamingChatMessageContentsAsync(history, settings, kernel, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to initialize stream for session {SessionId}", sessionId);
            setupError = ex;
        }

        if (setupError != null)
        {
            yield return ChatStreamEvent.CreateErrorEvent($"Failed to initialize stream: {setupError.Message}");
            yield break;
        }

        if (chunks is null)
        {
            _log.LogError("Stream chunks were null for session {SessionId}", sessionId);
            yield return ChatStreamEvent.CreateErrorEvent($"failed to initialize service provider streams");
            yield break;
        }

        // 2. Manual enumeration to safely catch streaming errors
        var enumerator = chunks.GetAsyncEnumerator(cancellationToken);
        var assistantMessage = new StringBuilder();
        Exception? streamError = null;

        try // Outer try-finally is ALLOWED to contain yield return
        {
            while (true)
            {
                bool hasMore;
                try // Inner try-catch has NO yield return, so it is ALLOWED
                {
                    hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error occurred while enumerating stream for session {SessionId}", sessionId);
                    streamError = ex;
                    break; // Safely exit the loop
                }

                if (!hasMore) break;

                var chunk = enumerator.Current;
                if (string.IsNullOrWhiteSpace(chunk.Content))
                {
                    continue;
                }

                assistantMessage.Append(chunk.Content);
                yield return ChatStreamEvent.CreateTokenEvent(chunk.Content);
            }
        }
        finally
        {
            // Ensures resources are cleaned up even if an error occurs
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }

        // 3. Yield the error event OUTSIDE the try-catch block (Compiler allows this)
        if (streamError != null)
        {
            yield return ChatStreamEvent.CreateErrorEvent($"Failed to get stream from provider: {streamError.Message}");
            yield break;
        }

        // 4. Success path
        _log.LogInformation("Stream completed successfully for session {SessionId}. Saving history.", sessionId);
        history.AddAssistantMessage(assistantMessage.ToString());
        await SaveHistoryAsync(sessionId, history).ConfigureAwait(false);

        yield return ChatStreamEvent.CreateCompletedEvent();
        yield return ChatStreamEvent.CreateSessionUpdateEvent(ChatSessionMapper.ToDTO(session));
    }


    private Kernel CreateKernel(Guid sessionId)
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(l =>
        {
            l.AddSerilog();
            l.SetMinimumLevel(LogLevel.Trace);
        });
        var cvFunctionsLogger = _loggerFactory.CreateLogger<ChatSessionService.CVFunctions>();
        builder.Plugins.AddFromObject(
            new CVFunctions(this, sessionId, cvFunctionsLogger));

        builder.AddOpenAIChatCompletion(
            modelId: "google/gemini-2.5-flash-lite",
            apiKey: Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"),
            endpoint: new Uri("https://openrouter.ai/api/v1"),
            httpClient: new HttpClient
            {
                DefaultRequestHeaders =
                {
                    { "HTTP-Referer", "http://localhost:5044" },
                    { "X-Title", "CV-App" }
                }
            });

        return builder.Build();
    }

    private static void EnsureSystemPrompt(ChatHistory history)
    {
        if (history.Any(x => x.Role == AuthorRole.System))
        {
            return;
        }

        history.AddSystemMessage(
            """
            You are a CV creation assistant. you Always use your tools to show stuff to user.

            The user sees the generated HTML rendered as PDF using WeasyPrint. 

            IMPORTANT:
            - Always generate valid semantic HTML.
            - Always include @page CSS rules.
            - Avoid overflow.
            - Prefer one-page layouts.
            - Keep spacing compact.
            - Optimize for PDF rendering.

            the user must not know anything about the html and do not tell the user
            anything about rendering or technical stuff.
            """
        );
    }

    public async Task<IEnumerable<ChatSessionDto>> GetAllAsync()
    {
        _log.LogInformation("Fetching all chat sessions");
        var results = await _db.ChatSessions.AsNoTracking()
            .Select(e => ChatSessionMapper.ToDTO(e))
            .ToListAsync().ConfigureAwait(false);
        _log.LogInformation("Successfully retrieved {Count} sessions", results.Count);
        return results;
    }

    public sealed class CVFunctions(
        IChatSessionService sessions,
        Guid sessionId,
        ILogger<ChatSessionService.CVFunctions> log)
    {
        private readonly IChatSessionService _sessions = sessions;
        private readonly Guid _sessionId = sessionId;
        private readonly ILogger<ChatSessionService.CVFunctions> _log = log;
        [KernelFunction("WriteCV")]
        [Description("Writes and saves the generated CV HTML. the written html will be rendered to the user")]
        public async Task<bool> WriteCVAsync(string html)
        {
            _log.LogInformation("AI Tool WriteCV invoked for session {SessionId}", _sessionId);
            await _sessions.UpdateHtmlAsync(_sessionId, html).ConfigureAwait(false);
            _log.LogInformation("Successfully wrote CV HTML for session {SessionId}", _sessionId);

            return true;
        }

        [KernelFunction("GetCV")]
        [Description("Returns the current saved CV HTML.")]
        public async Task<string> GetCurrentCVAsync()
        {
            _log.LogInformation("AI Tool GetCV invoked for session {SessionId}", _sessionId);
            var session = await _sessions.GetByIdAsync(_sessionId).ConfigureAwait(false);
            var html = session?.HtmlDocument ?? string.Empty;
            _log.LogDebug("Retrieved CV HTML for session {SessionId} (Length: {Length})", _sessionId, html.Length);

            return html;
        }
    }
}


