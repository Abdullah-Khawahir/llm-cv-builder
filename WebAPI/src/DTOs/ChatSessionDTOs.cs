using System.Text.Json.Serialization;

namespace WebAPI.DTOs;

public record class TitleModel(string title);

public sealed record ChatPromptRequest(
    string Prompt
);

public sealed record class ChatSessionListItemDto(
    Guid Id,
    string? Title,
    DateTime? UpdatedAt,
    DateTime CreatedAt
);
public sealed record class ChatSessionDetailsDto(
    Guid Id,
    string? Title,
    string HtmlDocument,
    ChatHistoryDto ChatHistory,
    uint? Version,
    string FontFamilyId = "dejavu"
);
public sealed record class ChatHistoryDto(
    IReadOnlyList<ChatMessageDto> Messages
);
public sealed record class ChatMessageDto(
    string Role,
    string Message
);
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Token), "token")]
[JsonDerivedType(typeof(ErrorEvent), "error")]
[JsonDerivedType(typeof(SessionUpdate), "session_update")]
[JsonDerivedType(typeof(Thinking), "thinking")]
[JsonDerivedType(typeof(Completed), "completed")]
public abstract record class ChatStreamEvent
{
    public sealed record class Token(string Content) : ChatStreamEvent;
    public sealed record class ErrorEvent(string Message) : ChatStreamEvent;
    public sealed record class SessionUpdate(ChatSessionDetailsDto ChatSessionDto) : ChatStreamEvent;
    public sealed record class Thinking(string Message = "thinking") : ChatStreamEvent;
    public sealed record class Completed(string Message = "completed") : ChatStreamEvent;

};

