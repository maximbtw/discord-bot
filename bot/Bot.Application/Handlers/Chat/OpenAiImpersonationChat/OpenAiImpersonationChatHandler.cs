using System.ClientModel;
using System.Text.RegularExpressions;
using Bot.Application.Infrastructure.Configuration;
using Bot.Contracts;
using Bot.Contracts.Handlers.AiChat;
using Bot.Contracts.Shared;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Handlers.Chat.OpenAiImpersonationChat;

internal class OpenAiImpersonationChatHandler : IAiChatHandler
{
    private readonly ICreatedMessageCache _messageCache;
    private readonly IMessageRepository _messageRepository;
    private readonly ChatClient _client;
    private readonly OpenAiImpersonationChatOptions _options;

    public OpenAiImpersonationChatHandler(
        ICreatedMessageCache messageCache,
        IConfiguration configuration,
        ChatClient client, 
        IMessageRepository messageRepository)
    {
        _messageCache = messageCache;
        _client = client;
        _messageRepository = messageRepository;
        _options = configuration.GetSection(nameof(OpenAiSettings)).Get<OpenAiSettings>()!.ChatOptions.ImpersonationChatOptions;
    }

    public async Task HandleMessage(
        DiscordClient discordClient, 
        MessageCreatedEventArgs args, 
        DbScope scope,
        CancellationToken ct)
    {
        var inputMessages = new List<ChatMessage>();

        List<MessageOrm> impersonationMessages = GetImpersonationMessages(args, scope);
        List<string> serverEmojis = args.Guild.Emojis.Values.Select(e => e.ToString()).ToList();

        var systemMessage = $"""
                             Твое имя "Джумпей". Не упоминай, что ты ИИ.

                             Ты должен отвечать в стиле пользователя из Discord.
                             Вот примеры его сообщений:
                             {string.Join("\n", impersonationMessages.Select(m => $"- {m.Content}").ToList())}

                             Иногда можешь использовать эмодзи сервера, но не злоупотребляй {string.Join(" ", serverEmojis)}
                             """;

        inputMessages.Add(new SystemChatMessage(systemMessage));

        IEnumerable<ChatMessage> historyMessages = LoadHistoryMessagesFromCache(args);
        inputMessages.AddRange(historyMessages);

        ChatMessage userMessage = new UserChatMessage(args.Message.Content)
        {
            ParticipantName = args.Author.Username,
        };

        inputMessages.Add(userMessage);

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 200,
            Temperature = 0.7f,
            TopP = 0.9f,
            FrequencyPenalty = 0.2f,
            PresencePenalty = 0.3f
        };

        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(inputMessages, options, ct);

        string responseText = result.Value.Content[0].Text;

        IAsyncEnumerable<DiscordMember> membersAsync = args.Guild.GetAllMembersAsync(ct);

        var userIndex = new Dictionary<string, ulong>();

        await foreach (DiscordMember member in membersAsync)
        {
            userIndex.Add(member.Username, member.Id);
        }

        responseText = ReplaceUserMentions(responseText, userIndex);

        await args.Message.RespondAsync(responseText);
    }

    private IEnumerable<ChatMessage> LoadHistoryMessagesFromCache(MessageCreatedEventArgs args)
    {
        List<MessageDto> cachedMessages = _messageCache.GetLastMessages(args.Guild.Id, args.Channel.Id)
            .Where(x => x.Id != (long)args.Message.Id)
            .TakeLast(10)
            .ToList();

        cachedMessages.Reverse();

        foreach (MessageDto cachedMessage in cachedMessages)
        {
            if (cachedMessage.UserIsBot)
            {
                yield return new AssistantChatMessage(Truncate(cachedMessage.Content, 200));
            }
            else
            {
                yield return new UserChatMessage(Truncate(cachedMessage.Content,200))
                {
                    ParticipantName = cachedMessage.UserName
                };
            }
        }
    }

    private List<MessageOrm> GetImpersonationMessages(MessageCreatedEventArgs args, DbScope scope)
    {
        List<MessageOrm> impersonationMessages = new List<MessageOrm>();

        IEnumerable<MessageOrm> messages = _messageRepository
            .GetQueryable(scope)
            .Where(x => x.ServerId == (long)args.Guild.Id);
        
        if (OpenAiImpersonationChatOptions.ImpersonationUseId != null)
        {
            messages = messages.Where(x => (ulong)x.UserId == OpenAiImpersonationChatOptions.ImpersonationUseId);
        }
        
        int maxOutputToken = 10000;
        int countToken = 0;
        foreach (MessageOrm userMessage in messages)
        {
            if (!MessageMatched(userMessage))
            {
                continue;
            }

            countToken += userMessage.Content!.Length;

            impersonationMessages.Add(userMessage);
            
            if (countToken > maxOutputToken)
            {
                break;
            }
        }

        return impersonationMessages;
    }
    
    private bool MessageMatched(MessageOrm message)
    {
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            return false;
        }

        string content = message.Content.Trim();
        
        if (content.Length is < 25 or > 300)
        {
            return false;
        }
        
        string[] words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 3)
        {
            return false;
        }
        
        if (content.StartsWith("/") || content.StartsWith("!") || content.StartsWith("."))
        {
            return false;
        }
        
        if (UrlRegex.IsMatch(content) || EmojiRegex.IsMatch(content))
        {
            return false;
        }
        
        if (Regex.IsMatch(content, @"(.)\1{5,}"))
        {
            return false;
        }
        
        int punctuationCount = content.Count(char.IsPunctuation);
        if (punctuationCount > content.Length * 0.3)
        {
            return false;
        }

        return true;
    }
    
    private static readonly Regex UrlRegex = new(@"https?://\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EmojiRegex = new(@":[a-zA-Z0-9_]+:|[\u2190-\u21FF\u2600-\u27BF\uE000-\uF8FF]", RegexOptions.Compiled);
    
    private static string Truncate(string value, int? maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (maxLength == null)
        {
            return value;
        }

        if (value.Length <= maxLength)
        {
            return value;
        }

        int lastSpace = value.LastIndexOf(' ', (int)maxLength);
        return lastSpace > 0 ? value[..lastSpace] : value[..(int)maxLength];
    }
    
    public static string ReplaceUserMentions(string text, Dictionary<string, ulong> userIds)
    {
        if (string.IsNullOrEmpty(text) || userIds.Count == 0)
            return text;
        
        var regex = new Regex(@"@(\w+)", RegexOptions.Compiled);

        var result = regex.Replace(text, match =>
        {
            var username = match.Groups[1].Value;
            if (userIds.TryGetValue(username, out ulong id))
            {
                return $"<@{id}>";
            }
            return match.Value;
        });

        return result;
    }
}