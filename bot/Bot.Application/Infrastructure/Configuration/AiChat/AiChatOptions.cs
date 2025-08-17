namespace Bot.Application.Infrastructure.Configuration.AiChat;

public class AiChatOptions
{
    public AiChatStrategy Strategy { get; set; }

    public AiChatOpenRouterSettings? OpenRouterSettings { get; set; } = null!;
}