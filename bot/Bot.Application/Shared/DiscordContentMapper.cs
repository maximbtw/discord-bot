using System.Text.RegularExpressions;
using Bot.Contracts.Message;
using Bot.Domain.Message;
using DSharpPlus.Entities;

namespace Bot.Application.Shared;

public static partial class DiscordContentMapper
{
    public static Message MapDiscordMessageToMessage(DiscordMessage message)
    {
        return new Message
        (
            Id: message.Id,
            UserId: message.Author!.Id,
            UserNickname: message.Author.Username,
            ChannelId: message.Channel!.Id,
            GuildId: (ulong)message.Channel.GuildId!,
            Content: MapContent(message),
            Timestamp: message.Timestamp.UtcDateTime,
            UserIsBot: message.Author.IsBot,
            ReplyToMessageId: message.ReferencedMessage?.Id,
            HasAttachments: message.Attachments.Count > 0,
            MentionedUserIds: message.MentionedUsers.Select(u => u.Id).ToList()
        );
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