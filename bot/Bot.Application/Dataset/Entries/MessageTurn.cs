using System.Text.Json.Serialization;

namespace Bot.Application.Dataset.Entries;

internal class MessageTurn
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty; 

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty; 
}