namespace WebAPI.DTOs;

public sealed record ChatPromptRequest(
    string Prompt
);

public sealed record ChatSessionDto(
    Guid Id,
    string HtmlDocument,
    ChatHistoryDto ChatHistory,
    uint? Version
);
public sealed record ChatHistoryDto(
    IReadOnlyList<ChatMessageDto> Messages
);
public sealed record ChatMessageDto(
    string Role,
    string Message
);

public sealed record ChatStreamEvent(string Type, string? Content = null, ChatSessionDto? ChatSessionDto = null)
{
    public static ChatStreamEvent Token(string content) => new("token", content);

    public static ChatStreamEvent Status(string content) => new("status", content);

    public static ChatStreamEvent Error(string content) => new("error", content);

    public static ChatStreamEvent UpdatedSession(ChatSessionDto session) => new("Updated", null, session);

    public static ChatStreamEvent Completed() => new("completed");

}
