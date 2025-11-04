using System.Text.RegularExpressions;
using Bot.Application.Chat.Services;
using DSharpPlus.Entities;
using OpenAI.Chat;

namespace Bot.Application.Chat;

public static class ChatHelper
{
    public static readonly Regex MentionRegex = new(@"(.)\1{5,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
        
        string result = MentionRegex.Replace(content, match =>
        {
            var username = match.Groups[1].Value;

            return userIndex.TryGetValue(username, out ulong id) ? $"<@{id}>" : match.Value;
        });

        return result;
    }
    
    public static bool IsValidChatMessage(DiscordMessage message)
    {
        return !string.IsNullOrWhiteSpace(message.Content) &&
               message.Author != null &&
               ShouldBotRespondToMessageType(message.MessageType);
    }
    
    public static bool ShouldBotRespondToMessageType(DiscordMessageType? type)
    {
        return type switch
        {
            DiscordMessageType.Default => true,
            DiscordMessageType.Reply => true,
            DiscordMessageType.ThreadStarterMessage => true,
            _ => false
        };
    }
}