using System.Runtime.CompilerServices;

namespace WebAPI.Services;

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


public sealed class ChatSessionService(
    ApplicationDbContext db,
    ILogger<ChatSessionService> log) : IChatSessionService
{
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger<ChatSessionService> _log = log;

    public async Task<ChatSessionDto?> GetByIdAsync(Guid id)
    {
        return await _db.ChatSessions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ChatSessionMapper.ToDTO(x))
            .FirstOrDefaultAsync();
    }

    public async Task<ChatSession?> GetEntityAsync(Guid id)
    {
        return await _db.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ChatSessionDto> CreateAsync()
    {
        var session = new ChatSession(Guid.NewGuid(), string.Empty, JsonSerializer.Serialize(new ChatHistory()), default);

        _db.ChatSessions.Add(session);

        await _db.SaveChangesAsync();

        return ChatSessionMapper.ToDTO(session);
    }

    public async Task UpdateHtmlAsync(Guid id, string html)
    {
        var session = await GetEntityAsync(id) ?? throw new InvalidOperationException("Session not found.");

        _db.Entry(session).CurrentValues.SetValues(session with { HtmlDocument = html });

        await _db.SaveChangesAsync();
    }

    public async Task SaveHistoryAsync(Guid id, ChatHistory history)
    {
        var session = await GetEntityAsync(id) ?? throw new InvalidOperationException("Session not found."); ;

        _db.Entry(session)
            .CurrentValues
            .SetValues(
                session with
                {
                    ChatHistoryJson = JsonSerializer.Serialize(history)
                });

        await _db.SaveChangesAsync();
    }

    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        Guid sessionId,
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var session = await GetEntityAsync(sessionId);
        if (session is null)
        {
            yield return ChatStreamEvent.CreateErrorEvent("Session not found.");
            yield break;
        }

        var history = session.ChatHistory;
        EnsureSystemPrompt(history);
        history.AddUserMessage(prompt);

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
            chunks = chatService.GetStreamingChatMessageContentsAsync(history, settings, kernel, cancellationToken);
        }
        catch (Exception ex)
        {
            setupError = ex;
        }

        if (setupError != null)
        {
            yield return ChatStreamEvent.CreateErrorEvent($"Failed to initialize stream: {setupError.Message}");
            yield break;
        }

        if (chunks is null)
        {
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
                    hasMore = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
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
            await enumerator.DisposeAsync();
        }

        // 3. Yield the error event OUTSIDE the try-catch block (Compiler allows this)
        if (streamError != null)
        {
            yield return ChatStreamEvent.CreateErrorEvent($"Failed to get stream from provider: {streamError.Message}");
            yield break;
        }

        // 4. Success path
        history.AddAssistantMessage(assistantMessage.ToString());
        await SaveHistoryAsync(sessionId, history);

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

        builder.Plugins.AddFromObject(
            new CVFunctions(this, sessionId));

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
        return await _db.ChatSessions.AsNoTracking()
            .Select(e => ChatSessionMapper.ToDTO(e))
            .ToListAsync();
    }

    public sealed class CVFunctions(
        IChatSessionService sessions,
        Guid sessionId)
    {
        private readonly IChatSessionService _sessions = sessions;
        private readonly Guid _sessionId = sessionId;

        [KernelFunction("WriteCV")]
        [Description("Writes and saves the generated CV HTML. the written html will be rendered to the user")]
        public async Task<bool> WriteCVAsync(string html)
        {
            await _sessions.UpdateHtmlAsync(_sessionId, html);

            return true;
        }

        [KernelFunction("GetCV")]
        [Description("Returns the current saved CV HTML.")]
        public async Task<string> GetCurrentCVAsync()
        {
            var session = await _sessions.GetByIdAsync(_sessionId);

            return session?.HtmlDocument ?? string.Empty;
        }
    }
}


