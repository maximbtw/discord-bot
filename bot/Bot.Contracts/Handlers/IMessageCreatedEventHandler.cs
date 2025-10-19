using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Contracts.Handlers;

public interface IMessageCreatedEventHandler
{
    Task Execute(DiscordClient client, MessageCreatedEventArgs args, DbScope scope);
}