using FluentMigrator;

namespace Bot.Domain.Migrations;

[Migration(0)]
public class M0000_CreateDatabase : Migration
{
    public override void Up()
    {
        Create.Table("Messages")
            .WithColumn("Id").AsInt64().PrimaryKey()
            .WithColumn("UserId").AsInt64().NotNullable()
            .WithColumn("UserName").AsString().NotNullable()
            .WithColumn("UserIsBot").AsBoolean().NotNullable()
            .WithColumn("ChannelId").AsInt64().NotNullable()
            .WithColumn("ServerId").AsInt64().NotNullable()
            .WithColumn("Content").AsString().Nullable()
            .WithColumn("Timestamp").AsDateTime().NotNullable()
            .WithColumn("ReplyToMessageId").AsInt64().Nullable()
            .WithColumn("MentionedUserIdsJson").AsString().Nullable()
            .WithColumn("HasAttachments").AsBoolean().NotNullable();
        
        Create.Index("IX_Messages_ServerId").OnTable("Messages").OnColumn("ServerId").Ascending();
        Create.Index("IX_Messages_ChannelId").OnTable("Messages").OnColumn("ChannelId").Ascending();
        Create.Index("IX_Messages_UserId").OnTable("Messages").OnColumn("UserId").Ascending();
    }

    public override void Down()
    {
        Delete.Table("DiscordMessages");
    }
}