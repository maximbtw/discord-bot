using System.ClientModel;
using System.Text.RegularExpressions;
using Bot.Application.Infrastructure.Configuration;
using Bot.Contracts;
using Bot.Contracts.Handlers.AiChat;
using Bot.Contracts.Services;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Handlers.Chat.OpenAiImpersonationChat;

internal class OpenAiImpersonationChatHandler : IAiChatHandler
{
    private readonly ICreatedMessageCache _messageCache;
    private readonly IMessageService _messageService;
    private readonly ChatClient _client;
    private readonly OpenAiImpersonationChatOptions _options;
    private readonly string _botName;

    public OpenAiImpersonationChatHandler(
        ICreatedMessageCache messageCache,
        IConfiguration configuration,
        ChatClient client, 
        IMessageService messageService)
    {
        _messageCache = messageCache;
        _client = client;
        _messageService = messageService;
        _options = configuration.GetSection(nameof(OpenAiSettings)).Get<OpenAiSettings>()!.ChatOptions.ImpersonationChatOptions;
        _botName = configuration.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>()!.BotName;
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

        var systemMessage = 
$"""
- Твое имя "{_botName}". Не упоминай, что ты ИИ.
- Ты должен отвечать в стиле пользователя из Discord. 
- Старайся не упоминать людей в чате когда это не трубется (крайне редко пиши @Username)
Вот примеры его сообщений:
{string.Join("\n", impersonationMessages.Select(m => $"- {m.Content}").ToList())}

- Иногда можешь использовать эмодзи сервера, но не злоупотребляй {string.Join(" ", serverEmojis)}
- У тебя есть история сообщений, пытайся поддерживать диалог.
""";

        inputMessages.Add(new SystemChatMessage(systemMessage));

        IEnumerable<ChatMessage> historyMessages = ChatHelper.LoadHistoryMessagesFromCache(
                _messageCache, 
                args.Guild.Id, 
                args.Channel.Id, 
                args.Message.Id, 
                _options.MaxHistoryMessageInputTokenCount,
                _options.MaxChatHistoryMessages);
        
        inputMessages.AddRange(historyMessages);

        ChatMessage userMessage = new UserChatMessage(args.Message.Content)
        {
            ParticipantName = args.Author.Username,
        };

        inputMessages.Add(userMessage);

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _options.MaxOutputTokenCount,
            Temperature = 0.7f,
            TopP = 0.9f,
            FrequencyPenalty = 0.2f,
            PresencePenalty = 0.3f
        };

        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(inputMessages, options, ct);

        string responseText = result.Value.Content[0].Text;

        if (_options.ReplaceMentions)
        {
            responseText = await ChatHelper.ReplaceUserMentions(responseText, args.Guild, ct);   
        }

        await args.Message.RespondAsync(responseText);
    }

    private List<MessageOrm> GetImpersonationMessages(MessageCreatedEventArgs args, DbScope scope)
    {
        List<MessageOrm> impersonationMessages = new List<MessageOrm>();

        IEnumerable<MessageOrm> messages = _messageService
            .GetQueryable(scope)
            .Where(x => x.GuildId == args.Guild.Id.ToString());
        
        if (_options.GuildIdToImpersonationUserIdIndex.TryGetValue(args.Guild.Id, out ulong userId))
        {
            messages = messages.Where(x => x.UserId == userId.ToString());
        }
        
        int countToken = 0;
        foreach (MessageOrm userMessage in messages)
        {
            if (!MessageMatched(userMessage))
            {
                continue;
            }

            countToken += userMessage.Content.Length;

            impersonationMessages.Add(userMessage);
            
            if (countToken > _options.MaxExampleMessagesTokenCount)
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
}