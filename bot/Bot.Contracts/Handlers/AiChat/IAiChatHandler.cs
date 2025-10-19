using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Contracts.Handlers.AiChat;

public interface IAiChatHandler
{
    Task HandleMessage(
        DiscordClient discordClient, 
        MessageCreatedEventArgs args, 
        DbScope scope,
        CancellationToken ct = default);
}