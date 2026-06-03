namespace WebAPI.Mappers;

public static class ChatSessionMapper
{
    public static ChatSessionDto ToDTO(ChatSession session)
    {
        var messages = session.ChatHistory
            .Skip(1)
            .Where(m => !string.IsNullOrWhiteSpace(m.Content))
            .Where(m => m.Role.Label.ToLower() == "user" || m.Role.Label.ToLower() == "assistant")
            .Select(m => new ChatMessageDto(
                m.Role.Label,
                m.Content ?? string.Empty
            ))
            .ToList();

        return new ChatSessionDto(
            session.Id,
            session.HtmlDocument,
            new ChatHistoryDto(messages),
            session.Version
        );
    }
}

