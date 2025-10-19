namespace Bot.Application.Handlers.Chat.OpenAiSimpleChat;

public class OpenAiSimpleChatOptions
{
    public string? SystemMessage { get; set; } = null!;

    public int MaxChatHistoryMessages { get; set; } = 20;
    
    public int MaxOutputTokenCount { get; set; }
    
    public int MaxInputTokenCount { get; set; }
}