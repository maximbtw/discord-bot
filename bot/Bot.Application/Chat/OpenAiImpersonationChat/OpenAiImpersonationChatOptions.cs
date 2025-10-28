using System.Collections.Concurrent;

namespace Bot.Application.Chat.OpenAiImpersonationChat;

public class OpenAiImpersonationChatOptions 
{
    public ConcurrentDictionary<ulong, ulong> GuildIdToImpersonationUserIdIndex { get; set; } = new();
    
    public int MaxChatHistoryMessages { get; set; } = 20;
    
    public int? MaxOutputTokenCount { get; set; }
    
    public int MaxHistoryMessageInputTokenCount { get; set; }
    
    public int MaxExampleMessagesTokenCount { get; set; }
    
    public bool ReplaceMentions { get; set; } = true;

    public string BotName { get; set; } = string.Empty;
}