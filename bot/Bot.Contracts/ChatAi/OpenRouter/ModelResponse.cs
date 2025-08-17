using System.Text.Json.Serialization;

namespace Bot.Contracts.ChatAi.OpenRouter;

public class ModelResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")] 
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")] 
    public List<Choice> Choices { get; set; } = new();
}

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; } = null!;

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}