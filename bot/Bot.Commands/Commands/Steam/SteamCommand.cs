using Bot.Application.Jobs.SteamNewReleasesLoader;
using Bot.Application.Jobs.SteamNewReleasesLoader.Service;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using Microsoft.Extensions.Configuration;

namespace Bot.Commands.Commands.Steam;

[Command("steam")]
internal partial class SteamCommand : ICommand
{
    private readonly ISteamNewReleasesService _steamNewReleasesService;
    private readonly SteamNewReleasesLoaderSettings _steamNewReleasesLoaderSettings;
    private readonly IDbScopeProvider _scopeProvider;

    public SteamCommand(
        ISteamNewReleasesService steamNewReleasesService,
        IConfiguration configuration,
        IDbScopeProvider scopeProvider)
    {
        _steamNewReleasesService = steamNewReleasesService;
        _scopeProvider = scopeProvider;

        _steamNewReleasesLoaderSettings = configuration.GetSection(nameof(SteamNewReleasesLoaderSettings))
            .Get<SteamNewReleasesLoaderSettings>()!;
    }
}