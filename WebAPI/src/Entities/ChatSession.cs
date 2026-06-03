using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Entities;

public sealed record ChatSession(
    Guid Id,
    string HtmlDocument,
    [property: Column(TypeName = "jsonb")] string ChatHistoryJson,
    [property: Timestamp] uint Version
    )
{
    [NotMapped]
    public ChatHistory ChatHistory
    {
        get => JsonSerializer.Deserialize<ChatHistory>(
                ChatHistoryJson,
                options: new JsonSerializerOptions
                {
                    AllowOutOfOrderMetadataProperties = true,
                }
                ) ?? new ChatHistory();
    }
}
