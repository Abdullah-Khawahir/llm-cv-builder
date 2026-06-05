using System.Text.Json.Serialization;

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
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Token), "token")]
[JsonDerivedType(typeof(Error), "error")]
[JsonDerivedType(typeof(SessionUpdate), "session_update")]
[JsonDerivedType(typeof(Thinking), "thinking")]
[JsonDerivedType(typeof(Completed), "completed")]
public abstract record ChatStreamEvent;

public sealed record Token(string Content) : ChatStreamEvent;
public sealed record Error(string Message) : ChatStreamEvent;
public sealed record SessionUpdate(ChatSessionDto ChatSessionDto) : ChatStreamEvent;
public sealed record Thinking(string Message = "thinking") : ChatStreamEvent;
public sealed record Completed(string Message = "completed") : ChatStreamEvent;

public static class ChatStreamEventFactory
{
    extension(ChatStreamEvent)
    {
        public static Token CreateTokenEvent(string token) => new(token);
        public static Error CreateErrorEvent(string message) => new(message);
        public static SessionUpdate CreateSessionUpdateEvent(ChatSessionDto session) => new(session);
        public static Thinking CreateThinkingEvent() => new();
        public static Completed CreateCompletedEvent() => new();
    }
}


