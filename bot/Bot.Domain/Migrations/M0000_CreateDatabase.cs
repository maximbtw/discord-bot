using FluentMigrator;

namespace Bot.Domain.Migrations;

[Migration(0)]
public class M0000_CreateDatabase : Migration
{
    public override void Up()
    {
        Create.Table("Messages")
            .WithColumn("Id").AsString(20).PrimaryKey()
            .WithColumn("UserId").AsString(20).NotNullable()
            .WithColumn("UserNickname").AsString(32).NotNullable()
            .WithColumn("UserIsBot").AsBoolean().NotNullable()
            .WithColumn("ChannelId").AsString(20).NotNullable()
            .WithColumn("GuildId").AsString(20).NotNullable()
            .WithColumn("Content").AsString().Nullable()
            .WithColumn("Timestamp").AsDateTime().NotNullable()
            .WithColumn("ReplyToMessageId").AsString(20).Nullable()
            .WithColumn("MentionedUserIdsJson").AsString().Nullable()
            .WithColumn("HasAttachments").AsBoolean().NotNullable();
        
        Create.Index("IX_Messages_GuildId").OnTable("Messages").OnColumn("GuildId").Ascending();
        Create.Index("IX_Messages_ChannelId").OnTable("Messages").OnColumn("ChannelId").Ascending();
        Create.Index("IX_Messages_UserId").OnTable("Messages").OnColumn("UserId").Ascending();
    }

    public override void Down()
    {
        Delete.Table("Messages");
    }
}