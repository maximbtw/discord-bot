using Bot.Domain.Scope;

namespace Bot.Domain.Orms.SteamNewReleasesSettings;

public interface ISteamNewReleasesSettingsRepository
{
    IQueryable<SteamNewReleasesSettingsOrm> GetQueryable(DbScope scope);
    
    Task Insert(SteamNewReleasesSettingsOrm orm, DbScope scope, CancellationToken ct);

    IQueryable<SteamNewReleasesSettingsOrm> GetUpdateQueryable(DbScope scope);
}