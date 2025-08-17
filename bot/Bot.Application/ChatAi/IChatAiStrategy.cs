using System.ComponentModel.DataAnnotations;
using Bot.Application.Infrastructure.Configuration.AiChat;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Application.ChatAi;

public interface IChatAiStrategy
{
    AiChatStrategy StrategyName { get; }
    
    int TimeOutInSeconds { get; }
    
    [Range(0,100)]
    int RandomMessageChance { get; }

    Task Execute(DiscordClient client, MessageCreatedEventArgs args, CancellationToken ct);
}