using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel.Connectors.Google;

namespace WebAPI.Services;

public interface IChatStreamingService
{
    IAsyncEnumerable<ChatStreamEvent> StreamAsync(Guid sessionId, string prompt, CancellationToken ct = default);
}

public sealed class ChatStreamingService(
    IChatSessionQueryService queries,
    IChatSessionCommandService commands,
    IKernelFactory kernelFactory,
    ILogger<ChatStreamingService> log) : IChatStreamingService
{
    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        Guid sessionId,
        string prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        log.LogInformation("Initializing stream for session {SessionId}", sessionId);

        var session = await queries.GetEntityAsync(sessionId).ConfigureAwait(false);

        if (session is null)
        {
            log.LogWarning("Stream failed: Session {SessionId} not found", sessionId);

            yield return ChatStreamEvent.CreateErrorEvent("Session not found.");
            yield break;
        }

        var history = session.ChatHistory;

        EnsureSystemPrompt(history);
        history.AddUserMessage(prompt);

        log.LogDebug("Creating kernel for session {SessionId}", sessionId);

        var kernel = kernelFactory.Create(sessionId);
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var settings = new GeminiPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Required(),
            ToolCallBehavior = GeminiToolCallBehavior.EnableKernelFunctions,
        };

        yield return ChatStreamEvent.CreateThinkingEvent();

        IAsyncEnumerator<StreamingChatMessageContent>? stream = null;
        Exception? setupError = null;

        try
        {
            log.LogDebug(
                "Requesting streaming contents from provider for session {SessionId}",
                sessionId);

            stream = chat.GetStreamingChatMessageContentsAsync(history, settings, kernel, ct)
                .GetAsyncEnumerator(ct);
        }
        catch (NotSupportedException ex)
        {
            log.LogError(
                ex,
                "Failed to initialize stream for session {SessionId}",
                sessionId);

            setupError = ex;
        }

        if (setupError is not null)
        {
            yield return ChatStreamEvent.CreateErrorEvent(
                $"Failed to initialize stream: {setupError.Message}");
            yield break;
        }

        if (stream is null)
        {
            log.LogError(
                "Stream enumerator was null for session {SessionId}",
                sessionId);

            yield return ChatStreamEvent.CreateErrorEvent(
                "Failed to initialize service provider stream.");

            yield break;
        }

        var assistantMessage = new StringBuilder();
        Exception? streamError = null;

        try
        {
            while (true)
            {
                bool hasMore;

                try
                {
                    hasMore = await stream.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log.LogError(
                        ex,
                        "Error occurred while enumerating stream for session {SessionId}",
                        sessionId);

                    streamError = ex;
                    break;
                }

                if (!hasMore)
                {
                    break;
                }

                var content = stream.Current.Content;

                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                assistantMessage.Append(content);

                yield return ChatStreamEvent.CreateTokenEvent(content);
            }
        }
        finally
        {
            await stream.DisposeAsync().ConfigureAwait(false);
        }
        if (streamError is not null)
        {
            yield return ChatStreamEvent.CreateErrorEvent(
                $"Failed to get stream from provider: {streamError.Message}");

            yield break;
        }

        log.LogInformation(
            "Stream completed successfully for session {SessionId}. Saving history.",
            sessionId);

        history.AddAssistantMessage(assistantMessage.ToString());

        await commands.SaveHistoryAsync(sessionId, history)
            .ConfigureAwait(false);

        yield return ChatStreamEvent.CreateCompletedEvent();

        yield return ChatStreamEvent.CreateSessionUpdateEvent(
            ChatSessionMapper.ToDTO(session));
    }

    private static void EnsureSystemPrompt(ChatHistory history)
    {
        if (!history.Any(x => x.Role == AuthorRole.System))
            history.AddSystemMessage(Prompts.USER_INTERACTION_AGENT_SYS_PROMPT);
    }
}
