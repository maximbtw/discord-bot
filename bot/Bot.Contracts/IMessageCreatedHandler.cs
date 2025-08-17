using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Contracts;

public interface IMessageCreatedHandler
{
    ValueTask<bool> NeedExecute(DiscordClient client, MessageCreatedEventArgs args);
    
    Task Execute(DiscordClient client, MessageCreatedEventArgs args);
}