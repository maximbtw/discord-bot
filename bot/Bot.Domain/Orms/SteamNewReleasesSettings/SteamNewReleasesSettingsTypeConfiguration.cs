using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bot.Domain.Orms.SteamNewReleasesSettings;

public class SteamNewReleasesSettingsTypeConfiguration: IEntityTypeConfiguration<SteamNewReleasesSettingsOrm>
{
    public void Configure(EntityTypeBuilder<SteamNewReleasesSettingsOrm> builder)
    {
    }
}