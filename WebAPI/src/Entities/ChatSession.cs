using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Entities;

public class ChatSession
{
    public Guid Id { get; set; }
    public string HtmlDocument { get; set; } = String.Empty;

    [Column(TypeName = "jsonb")]
    public string ChatHistoryJson { get; set; } = String.Empty;

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
