using System.Net.Http.Json;
using System.Text.Json;
using Bot.Application.Jobs.SteamNewReleasesLoader;
using Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;
using Bot.Application.Jobs.SteamNewReleasesLoader.Service;
using Bot.Commands.Checks.RequireApplicationOwner;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using HtmlAgilityPack;

namespace Bot.Commands.Commands;

public class TestCommand : ICommand
{
    private readonly ISteamNewReleasesService _steamNewReleasesService;

    public TestCommand(ISteamNewReleasesService steamNewReleasesService)
    {
        _steamNewReleasesService = steamNewReleasesService;
    }

    [Command("test")]
    [MyRequireApplicationOwner]
    public async ValueTask Execute(CommandContext context)
    {
        await context.DeferResponseAsync();
        
        IAsyncEnumerable<string> appIds = _steamNewReleasesService.GetLastAppIds(1);
        await foreach (string appId in appIds)
        {
            SteamAppDetailsResponse? details = await _steamNewReleasesService.GetAppDetails(appId, "RU", "russia");
            
            DiscordEmbed embed = SteamNewReleasesLoaderDiscordEmbedBuilder.Build(appId, details.Data);
        
            await context.RespondAsync(embed);
            
            break;
        }
    }
}