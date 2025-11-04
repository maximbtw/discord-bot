using FluentMigrator;

namespace Bot.Domain.Migrations;

[Migration(2)]
public class M0002_ChatSettings: Migration
{
    public override void Up()
    {
        Create.Table("ChatSettings")
            .WithColumn("GuildId").AsString(20).NotNullable().PrimaryKey()
            .WithColumn("ChatType").AsByte().NotNullable()
            .WithColumn("ResponseChance").AsInt32().Nullable()
            .WithColumn("ChatHistoryLimit").AsInt32().Nullable()
            .WithColumn("ReplaceMentions").AsBoolean().NotNullable()
            .WithColumn("ImpersonationUserId").AsString().Nullable();
        
        Create.Index("IX_ChatSettings_GuildId").OnTable("ChatSettings").OnColumn("GuildId").Ascending();
    }

    public override void Down()
    {
        Delete.Table("ChatSettings");
    }
}