using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Entities;

public sealed record class ChatSession(
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
                options: s_jsonSerializerOptions
                ) ?? new ChatHistory();
    }
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { AllowOutOfOrderMetadataProperties = true };
}
