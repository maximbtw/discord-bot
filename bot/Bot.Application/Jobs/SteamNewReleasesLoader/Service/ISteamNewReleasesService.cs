using Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;
using Bot.Domain.Orms.SteamNewReleasesSettings;
using Bot.Domain.Scope;

namespace Bot.Application.Jobs.SteamNewReleasesLoader.Service;

public interface ISteamNewReleasesService
{
    IAsyncEnumerable<string> GetLastAppIds(
        int releaseCount, 
        string? cc = null, 
        string? language = null,
        CancellationToken ct = default);

    Task<SteamAppDetailsResponse?> GetAppDetails(
        string appId, 
        string? cc = null,
        string? language = null,
        CancellationToken ct = default);

    IQueryable<SteamNewReleasesSettingsOrm> GetSettings(DbScope scope);

    Task AddOrUpdateGuildSettings(ulong guildId, ulong channelId, DbScope scope, CancellationToken ct = default);

    Task<bool> TryPauseProcessOnGuild(ulong guildId, DbScope scope, CancellationToken ct = default);
    
    Task UpdateLastLoadedApp(string appId, List<string> guildIds, DbScope scope, CancellationToken ct = default);
}