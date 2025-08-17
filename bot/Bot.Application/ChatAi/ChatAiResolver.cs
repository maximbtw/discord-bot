using Bot.Application.Infrastructure.Configuration.AiChat;

namespace Bot.Application.ChatAi;

internal class ChatAiResolver
{
    private readonly Dictionary<AiChatStrategy, IChatAiStrategy> _strategies;
    
    public ChatAiResolver(IEnumerable<IChatAiStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(x => x.StrategyName);
    }
    
    public IChatAiStrategy Resolve(AiChatStrategy strategyName)
    {
        if (_strategies.TryGetValue(strategyName, out IChatAiStrategy? strategy))
        {
            return strategy;
        }

        throw new NotSupportedException($"Strategy {strategyName} is not supported");
    }
}