using System.ComponentModel;
using Bot.Application.Jobs.SteamNewReleasesLoader;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Quartz;

namespace Bot.Commands.Commands.Steam;

internal partial class SteamCommand
{
    [Command("start")]
    [Description("Начинает отслеживать новые игры в Steam и публиковать co-op релизы в канал.")]
    [RequireGuild]
    [RequirePermissions([],[DiscordPermission.Administrator])]
    public async ValueTask ExecuteNewGamesStart(
        CommandContext context, 
        [Description("Канал для публикации.")] DiscordChannel channel)
    {
        await context.DeferResponseAsync();

        if (!_steamNewReleasesLoaderSettings.Enabled)
        {
            await context.RespondAsync("Процесс сейчас не работает.");
            
            return;
        }

        await using DbScope scope = _scopeProvider.GetDbScope();
        
        await _steamNewReleasesService.AddOrUpdateGuildSettings(context.Guild!.Id, channel.Id, scope);
        
        await context.RespondAsync($"Процесс запущен в канале: {channel.Name}");
    }
}