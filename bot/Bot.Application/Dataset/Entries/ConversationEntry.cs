using System.Text.Json.Serialization;

namespace Bot.Application.Dataset.Entries;

internal class ConversationEntry
{
    [JsonPropertyName("conversations")]
    public List<MessageTurn> Conversations { get; set; } = new();
}