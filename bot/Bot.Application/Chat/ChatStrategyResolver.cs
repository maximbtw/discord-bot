using Bot.Domain.Orms.ChatSettings;

namespace Bot.Application.Chat;

public class ChatStrategyResolver
{
    private readonly Dictionary<ChatType, IChatStrategy> _strategies;
    
    public ChatStrategyResolver(IEnumerable<IChatStrategy> chatStrategies)
    {
        _strategies = chatStrategies.ToDictionary(x => x.Type);
    }
    
    public IChatStrategy Resolve(ChatType chatType)
    {
        return _strategies[chatType];
    }
}