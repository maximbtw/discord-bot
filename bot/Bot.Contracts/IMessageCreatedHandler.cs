using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Contracts;

public interface IMessageCreatedHandler
{
    Task Execute(DiscordClient client, MessageCreatedEventArgs args);
}