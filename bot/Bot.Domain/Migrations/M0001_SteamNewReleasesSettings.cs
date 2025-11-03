using FluentMigrator;

namespace Bot.Domain.Migrations;

[Migration(1)]
public class M0001_SteamNewReleasesSettings: Migration
{
    public override void Up()
    {
        Create.Table("SteamNewReleasesSettings")
            .WithColumn("GuildId").AsString(20).NotNullable().PrimaryKey()
            .WithColumn("ChannelId").AsString(20).NotNullable()
            .WithColumn("Pause").AsBoolean().NotNullable()
            .WithColumn("LastLoadedAppId").AsString(10).Nullable()
            .WithColumn("LastLoadedAppDateTime").AsDateTime().Nullable();
        
        Create.Index("IX_SteamNewReleasesSettings_GuildId").OnTable("SteamNewReleasesSettings").OnColumn("GuildId").Ascending();
        Create.Index("IX_SteamNewReleasesSettings_ChannelId").OnTable("SteamNewReleasesSettings").OnColumn("ChannelId").Ascending();
    }

    public override void Down()
    {
        Delete.Table("SteamNewReleasesSettings");
    }
}