using System.Text.Json.Serialization;

namespace Bot.Contracts.Ai.OpenRouter;

public class ModelRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "openai/gpt-oss-20b:free";

    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();

    [JsonPropertyName("user")] 
    public string User { get; set; } = string.Empty;
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; }  = string.Empty; // "user", "assistant", "system"

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}