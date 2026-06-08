namespace WebAPI.Mappers;

public static class ChatSessionMapper
{
    public static ChatSessionDetailsDto ToDTO(ChatSession session)
    {
        var messages = session.ChatHistory
            .Skip(1)
            .Where(m =>
                    !string.IsNullOrWhiteSpace(m.Content) &&
                    (m.Role.Label.Equals("user", StringComparison.Ordinal) ||
                     m.Role.Label.Equals("assistant", StringComparison.Ordinal))
                    )
            .Select(m => new ChatMessageDto(
                m.Role.Label,
                m.Content ?? string.Empty
            ))
            .ToList();

        return new ChatSessionDetailsDto(
            session.Id,
            session.Title,
            session.HtmlDocument,
            new ChatHistoryDto(messages),
            session.Version
        );
    }

    public static ChatSessionListItemDto ToListItemDTO(ChatSession session)
    {
        return new ChatSessionListItemDto(
            session.Id,
            session.Title ?? "Untitled"
        );
    }
}

