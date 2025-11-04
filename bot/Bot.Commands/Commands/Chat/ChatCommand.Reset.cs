using System.ComponentModel;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Bot.Commands.Commands.Chat;

internal partial class ChatCommand
{
    [Command("reset-cache")]
    [Description("Чистит кэш переписки. Восстанавливает стандартные настройки чата.")]
    [RequireGuild]
    [RequirePermissions([],[DiscordPermission.Administrator])]
    public async ValueTask ExecuteResetCache(
        CommandContext context, 
        [Description("Канал для которого стоит отчистить кэш.")] DiscordChannel? channel = null)
    {
        DiscordGuild guild = context.Guild!;

        var channelsIds = new List<ulong>();
        if (channel is not null)
        {
            channelsIds.Add(channel.Id);
        }
        else
        {
            channelsIds.AddRange(guild.Channels.Keys);
        }

        _chatService.ResetCache(guild.Id, channelsIds);

        await context.RespondAsync("Кэш переписки сброшен.");
    }
    
    [Command("reset-settings")]
    [Description("Восстанавливает стандартные настройки чата.")]
    [RequireGuild]
    [RequirePermissions([],[DiscordPermission.Administrator])]
    public async ValueTask ExecuteResetSettings(CommandContext context)
    {
        DiscordGuild guild = context.Guild!;

        await using DbScope scope = _scopeProvider.GetDbScope();

        await _chatService.ResetChatSettings(guild.Id, scope);

        await scope.CommitAsync();

        await context.RespondAsync("Настройки чата успешно сброшены.");
    }
}