using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;

namespace WebAPI.Entities;

public sealed class ChatSession : AuditableEntity
{
    public Guid Id { get; init; }
    public string? Title { get; set; }
    public string HtmlDocument { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string ChatHistoryJson { get; set; } = "[]";

    [Timestamp]
    public uint Version { get; private set; }


    [NotMapped]
    public ChatHistory ChatHistory
    {
        get => !string.IsNullOrWhiteSpace(ChatHistoryJson)
            ? JsonSerializer.Deserialize<ChatHistory>(ChatHistoryJson, s_jsonSerializerOptions) ?? new()
            : new();
        set
        {
            ChatHistoryJson = JsonSerializer.Serialize(value, s_jsonSerializerOptions);
        }
    }

    [NotMapped]
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { AllowOutOfOrderMetadataProperties = true };
}
