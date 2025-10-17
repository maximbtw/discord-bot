using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Contracts;

public interface IChatService
{
    int RandomMessageChance { get; }
    
    Task HandleMessage(DiscordClient discordClient, MessageCreatedEventArgs args, CancellationToken ct);
}