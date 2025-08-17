using System.Text.Json.Serialization;

namespace Bot.Contracts.ChatAi.OpenRouter;

public class ModelRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "openai/gpt-oss-20b:free";

    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();

    [JsonPropertyName("user")] 
    public string User { get; set; } = string.Empty;
}
