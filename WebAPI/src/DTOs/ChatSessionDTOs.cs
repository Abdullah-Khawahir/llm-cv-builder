using System.Text.Json.Serialization;

namespace WebAPI.DTOs;

public sealed record ChatPromptRequest(
    string Prompt
);

public sealed record class ChatSessionDto(
    Guid Id,
    string HtmlDocument,
    ChatHistoryDto ChatHistory,
    uint? Version
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
public abstract record class ChatStreamEvent;

public sealed record class Token(string Content) : ChatStreamEvent;
public sealed record class ErrorEvent(string Message) : ChatStreamEvent;
public sealed record class SessionUpdate(ChatSessionDto ChatSessionDto) : ChatStreamEvent;
public sealed record class Thinking(string Message = "thinking") : ChatStreamEvent;
public sealed record class Completed(string Message = "completed") : ChatStreamEvent;

public static class ChatStreamEventFactory
{
    extension(ChatStreamEvent)
    {
        public static Token CreateTokenEvent(string token) => new(token);
        public static ErrorEvent CreateErrorEvent(string message) => new(message);
        public static SessionUpdate CreateSessionUpdateEvent(ChatSessionDto session) => new(session);
        public static Thinking CreateThinkingEvent() => new();
        public static Completed CreateCompletedEvent() => new();
    }
}


