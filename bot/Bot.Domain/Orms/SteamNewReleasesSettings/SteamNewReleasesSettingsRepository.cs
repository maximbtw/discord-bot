using Bot.Domain.Scope;
using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.Orms.SteamNewReleasesSettings;

public class SteamNewReleasesSettingsRepository : ISteamNewReleasesSettingsRepository
{
    public IQueryable<SteamNewReleasesSettingsOrm> GetQueryable(DbScope scope)
    {
        DiscordDbContext context = scope.GetDbContext();
        
        return context.Set<SteamNewReleasesSettingsOrm>().AsNoTracking();
    }

    public async Task Insert(SteamNewReleasesSettingsOrm orm, DbScope scope, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orm);
        
        DiscordDbContext context = scope.GetDbContext();

        await context.AddAsync(orm, ct);
    }

    public IQueryable<SteamNewReleasesSettingsOrm> GetUpdateQueryable(DbScope scope)
    {
        DiscordDbContext context = scope.GetDbContext();

        return context.Set<SteamNewReleasesSettingsOrm>();
    }
}