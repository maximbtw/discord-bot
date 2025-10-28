using System.Text.RegularExpressions;
using Bot.Contracts.Services;
using Bot.Contracts.Shared;
using DSharpPlus.Entities;
using OpenAI.Chat;

namespace Bot.Application.Chat;

public static class ChatHelper
{
    public static IEnumerable<ChatMessage> LoadHistoryMessagesFromCache(
        IMessageService messageService,
        ulong guildId,
        ulong channelId,
        ulong newMessageId,
        int messageContentMaxLength,
        int maxMessages)
    {
        List<Message> cachedMessages = messageService.GetMessagesFromCache(guildId, channelId)
            .Where(x => x.Id != newMessageId)
            .TakeLast(maxMessages)
            .ToList();

        cachedMessages.Reverse();

        foreach (Message cachedMessage in cachedMessages)
        {
            if (cachedMessage.UserIsBot)
            {
                string content = TruncateMessageContent(cachedMessage.Content, messageContentMaxLength);
                
                yield return new AssistantChatMessage(content);
            }
            else
            {
                string content = TruncateMessageContent(cachedMessage.Content, messageContentMaxLength);
                
                yield return new UserChatMessage(content)
                {
                    ParticipantName = cachedMessage.UserNickname
                };
            }
        }
    }

    public static string TruncateMessageContent(string content, int? maxLength)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        if (maxLength == null || content.Length <= maxLength)
        {
            return content;
        }

        int lastSpace = content.LastIndexOf(' ', (int)maxLength);
        return lastSpace > 0 ? content[..lastSpace] : content[..(int)maxLength];
    }

    public static async Task<string> ReplaceUserMentions(
        string content, 
        DiscordGuild guild,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        IAsyncEnumerable<DiscordMember> membersAsync = guild.GetAllMembersAsync(ct);

        var userIndex = new Dictionary<string, ulong>();

        await foreach (DiscordMember member in membersAsync)
        {
            userIndex.Add(member.Username, member.Id);
        }

        var regex = new Regex(@"@(\w+)", RegexOptions.Compiled);
        string result = regex.Replace(content, match =>
        {
            var username = match.Groups[1].Value;

            return userIndex.TryGetValue(username, out ulong id) ? $"<@{id}>" : match.Value;
        });

        return result;
    }
}