namespace WebAPI.Services;

public class ChatSessionService : IChatSessionService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ChatSessionService> _log;

    public ChatSessionService(ApplicationDbContext db, Kernel kernel, ILogger<ChatSessionService> log)
    {
        _db = db;
        _log = log;
    }

    public async IAsyncEnumerable<string> ProcessPromptStreamingAsync(Guid id, string prompt)
    {
        var session = await GetSessionForPromptAsync(id);
        if (session is null)
        {
            yield return "Error: Session not found.";
            yield break;
        }

        var history = JsonSerializer.Deserialize<ChatHistory>(session.ChatHistoryJson,
            new JsonSerializerOptions { AllowOutOfOrderMetadataProperties = true }) ?? new ChatHistory();

        if (history.FirstOrDefault(h => h.Role == AuthorRole.System) is null)
        {
            history.AddSystemMessage("you are a CV creating assistant. you will make CV for the user by calling the function to write the CV.");
        }

        history.AddUserMessage(prompt);

        var settings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        var kernalBuilder = Kernel.CreateBuilder();
        kernalBuilder.Plugins.AddFromObject(new SessionFunctions(session, _db));
        kernalBuilder.AddOpenAIChatCompletion(
            modelId: "openai/gpt-oss-120b:free",
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

        var kernel = kernalBuilder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // 2. Track the full response string to update DB later
        var fullResponseBuilder = new StringBuilder();

        // 3. Invoke streaming. Semantic Kernel handles tool calls automatically behind the scenes,
        // but chunks that represent function arguments won't usually yield text content to the user.
        await foreach (var chatChunk in chatService.GetStreamingChatMessageContentsAsync(history, settings, kernel))
        {
            if (!string.IsNullOrEmpty(chatChunk.Content))
            {
                fullResponseBuilder.Append(chatChunk.Content);
                yield return chatChunk.Content; // Stream text back to controller/client immediately
            }
        }

        // 4. After the stream completes, save the aggregated response to history
        history.AddAssistantMessage(fullResponseBuilder.ToString());

        // We reload session just in case 'SessionFunctions' modified it during the stream execution
        var updatedSession = await GetSessionForPromptAsync(id);
        if (updatedSession != null)
        {
            updatedSession.ChatHistoryJson = JsonSerializer.Serialize(history);
            await _db.SaveChangesAsync();
        }
    }



    public async Task<ICollection<ChatSession>> GetAllSessionsAsync()
    {
        return await _db.ChatSessions.AsNoTracking().ToListAsync();
    }

    public async Task<ChatSession?> GetByIdAsync(Guid id)
    {
        return await _db.ChatSessions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ChatSession?> GetSessionForPromptAsync(Guid id)
    {
        return await _db.ChatSessions.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ChatSession> CreateNewAsync()
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            HtmlDocument = string.Empty,
            ChatHistoryJson = JsonSerializer.Serialize(new ChatHistory()),
        };
        await _db.ChatSessions.AddAsync(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<ChatSession?> ProcessPromptAsync(Guid id, string prompt)
    {
        var session = await GetSessionForPromptAsync(id);
        if (session is null) return null;

        var history = JsonSerializer.Deserialize<ChatHistory>(session.ChatHistoryJson,
            new JsonSerializerOptions { AllowOutOfOrderMetadataProperties = true }) ?? new ChatHistory();

        if (history.FirstOrDefault(h => h.Role == AuthorRole.System) is null)
        {
            history.AddSystemMessage("you are a CV creating assistant. you will make CV for the user by calling the function to write the CV.");
        }

        history.AddUserMessage(prompt);

        var settings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        var kernalBuilder = Kernel.CreateBuilder();

        kernalBuilder.Plugins.AddFromObject(new SessionFunctions(session, _db));
        kernalBuilder.AddOpenAIChatCompletion(
            modelId: "openai/gpt-oss-120b:free",
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

        var kernel = kernalBuilder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var response = await chatService.GetChatMessageContentAsync(history, settings, kernel);

        history.AddAssistantMessage(response.Content ?? string.Empty);

        var historyJson = JsonSerializer.Serialize(history);
        session.ChatHistoryJson = historyJson;

        await _db.SaveChangesAsync();
        return session;
    }


    public class SessionFunctions
    {
        private readonly ChatSession _session;
        private readonly ApplicationDbContext _db;
        public SessionFunctions(ChatSession session, ApplicationDbContext db)
        {
            _session = session;
            _db = db;
        }

        [KernelFunction("WriteCV")]
        [Description("Writes the provided HTML and saves it.")]
        public async Task<bool> WriteCVAsync(string html)
        {
            _session.HtmlDocument = html;
            _db.ChatSessions.Update(_session);
            await _db.SaveChangesAsync();
            return true;
        }

        [KernelFunction("GetCV")]
        [Description("return the currently saved CV")]
        public string GetCurrentCVAsync()
        {
            return _session.HtmlDocument;
        }
    }
}


