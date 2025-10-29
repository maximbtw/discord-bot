using System.ComponentModel;
using Bot.Application.Chat.OpenAiImpersonationChat;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Bot.Commands.Commands.Chat;

internal partial class ChatCommand
{
    [Command("reset")]
    [Description("Чистит кэш переписки. Восстанавливает стандартные настройки чата.")]
    [RequireGuild]
    [RequirePermissions([],[DiscordPermission.Administrator])]
    public async ValueTask ExecuteReset(
        CommandContext context, 
        [Description("Канал для которого стоит отчистить кэш.")] DiscordChannel? channel = null)
    {
        DiscordGuild guild = context.Guild!;
        
        OpenAiImpersonationChatOptions.GuildIdToImpersonationUserIdIndex.TryRemove(guild.Id, out _);

        var channelsIds = new List<ulong>();
        if (channel is not null)
        {
            channelsIds.Add(channel.Id);
        }
        else
        {
            channelsIds.AddRange(guild.Channels.Keys);
        }

        _messageService.ClearCache(guild.Id, channelsIds);

        await context.RespondAsync("Кэш переписки и настройки чата успешно сброшены.");
    }
}