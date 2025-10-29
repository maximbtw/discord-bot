using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Application.Chat;

public interface IChatStrategy
{
    ChatType Type { get; }

    Task Execute(DiscordClient client, MessageCreatedEventArgs args, DbScope scope, CancellationToken ct);
}
