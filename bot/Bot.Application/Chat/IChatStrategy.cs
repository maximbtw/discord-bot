using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Application.Chat;

public interface IChatStrategy
{
    ChatType Type { get; }

    Task Execute(DiscordClient client, MessageCreatedEventArgs args, GuildChatSettings guildSettings, CancellationToken ct);
}
