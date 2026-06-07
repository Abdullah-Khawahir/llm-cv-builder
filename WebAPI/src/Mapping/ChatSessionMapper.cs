namespace WebAPI.Mappers;

public static class ChatSessionMapper
{
    public static ChatSessionDto ToDTO(ChatSession session)
    {
        var messages = session.ChatHistory
            .Skip(1)
            .Where(m => !string.IsNullOrWhiteSpace(m.Content) && (m.Role.Label.Equals("user", StringComparison.Ordinal) || m.Role.Label.Equals("assistant", StringComparison.Ordinal))).Select(m => new ChatMessageDto(
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

