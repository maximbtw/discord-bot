using System.Text.Json.Serialization;

namespace Bot.Contracts.ChatAi.OpenRouter;

public class Message
{
    // "user", "assistant", "system"
    [JsonPropertyName("role")]
    public string Role { get; set; }  = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}