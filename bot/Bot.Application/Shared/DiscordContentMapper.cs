using System.Text.RegularExpressions;
using Bot.Contracts.Shared;
using Bot.Domain.Message;
using DSharpPlus.Entities;

namespace Bot.Application.Shared;

internal static partial class DiscordContentMapper
{
    public static MessageDto MapDiscordMessageToDto(DiscordMessage message)
    {
        return new MessageDto
        (
            Id: (long)message.Id,
            UserId: (long)message.Author!.Id,
            UserName: message.Author.Username,
            ChannelId: (long)message.Channel!.Id,
            ServerId: (long)message.Channel.GuildId!,
            Content: MapContent(message),
            Timestamp: message.Timestamp.UtcDateTime,
            UserIsBot: message.Author.IsBot,
            ReplyToMessageId: (long?)message.ReferencedMessage?.Id,
            HasAttachments: message.Attachments.Count > 0,
            MentionedUserIds: message.MentionedUsers.Select(u => (long)u.Id).ToList()
        );
    }

    public static MessageOrm MapDiscordMessage(DiscordMessage message)
    {
        return new MessageOrm
        {
            Id = (long)message.Id,
            UserId = (long)message.Author!.Id,
            UserName = message.Author.Username,
            ChannelId =(long) message.Channel!.Id,
            ServerId = (long)message.Channel.GuildId!,
            Content = MapContent(message),
            Timestamp = message.Timestamp.UtcDateTime,
            UserIsBot = message.Author.IsBot,
            ReplyToMessageId = (long?)message.ReferencedMessage?.Id,
            HasAttachments = message.Attachments.Count > 0,
            MentionedUserIds = message.MentionedUsers.Select(u => (long)u.Id).ToList()
        };
    }

    public static string MapContent(DiscordMessage message)
    {
        return UsernameRegex().Replace(message.Content, match =>
        {
            if (ulong.TryParse(match.Groups[1].Value, out ulong userId))
            {
                DiscordUser? user = message.MentionedUsers.FirstOrDefault(x => x.Id == userId);
                if (user != null)
                {
                    return $"@{user.Username}";
                }
            }
            return match.Value; 
        });
    }

    [GeneratedRegex(@"<@!?(\d+)>")]
    private static partial Regex UsernameRegex();
}