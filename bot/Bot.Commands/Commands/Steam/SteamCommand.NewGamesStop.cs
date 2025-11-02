using System.ComponentModel;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Bot.Commands.Commands.Steam;

internal partial class SteamCommand
{
    [Command("newGamesStop")]
    [Description("Перестает отслеживать новые игры в Steam и публиковать co-op релизы в канал.")]
    [RequireGuild]
    [RequirePermissions([],[DiscordPermission.Administrator])]
    public async ValueTask ExecuteNewGamesStop(CommandContext context)
    {
        await context.DeferResponseAsync();

        if (!_steamNewReleasesLoaderSettings.Enabled)
        {
            await context.RespondAsync("Процесс сейчас не работает.");
            
            return;
        }

        await using DbScope scope = _scopeProvider.GetDbScope();
        
        bool paused = await _steamNewReleasesService.TryPauseProcessOnGuild(context.Guild!.Id, scope);
        if (paused)
        {
            await context.RespondAsync("Процесс уже был остановлен.");
            return;
        }

        await context.RespondAsync("Процесс успешно остановлен!");
    }
}