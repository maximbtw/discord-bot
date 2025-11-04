using System.Text.RegularExpressions;
using Bot.Application.Chat.Services;
using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using Bot.Domain.Orms.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Chat.ImpersonationChat;

internal class ImpersonationChatStrategy  : ChatStrategyBase<ImpersonationChatOptions>
{
    private const int MaxEmojisInMessage = 20;
    private const int MinContentLength = 15;
    private const int MaxContentLength = 300;
    
    private readonly IDbScopeProvider _scopeProvider;
    private readonly IMemoryCache _memoryCache;
    
    public ImpersonationChatStrategy(
        IConfiguration configuration, 
        ChatClient client,
        IChatService chatService, 
        IDbScopeProvider scopeProvider, 
        IMemoryCache memoryCache) : base(chatService, client, configuration)
    {
        _scopeProvider = scopeProvider;
        _memoryCache = memoryCache;
    }
    
    public override ChatType Type => ChatType.Impersonation;

    protected override ImpersonationChatOptions GetOptions(ChatSettings settings) => settings.ImpersonationChatOptions;

    protected override async ValueTask<List<SystemChatMessage>> CreateSystemChatMessages(
        DiscordClient client, 
        MessageCreatedEventArgs args, 
        GuildChatSettings guildSettings,
        CancellationToken ct)
    {
        string key = $"{nameof(ImpersonationChatStrategy)}:{args.Guild.Id}";
        
        bool shouldUpdateCache = 
            !_memoryCache.TryGetValue(key, out CacheValue value) ||
            value.UserId != guildSettings.ImpersonationUserId;

        if (shouldUpdateCache)
        {
            string systemMessage = await CreateSystemMessage(args, guildSettings, ct);

            value = new CacheValue(guildSettings.ImpersonationUserId, systemMessage);
            
            _memoryCache.Set(key, value, absoluteExpirationRelativeToNow: TimeSpan.FromHours(2));
        }

        return [ new SystemChatMessage(value.SystemMessage) ];
    }

    private async Task<string> CreateSystemMessage(
        MessageCreatedEventArgs args, 
        GuildChatSettings guildSettings,
        CancellationToken ct)
    {
        ImpersonationChatOptions options = GetOptions();

        await using DbScope scope = _scopeProvider.GetDbScope();

        List<string> userMessages = await GetImpersonationMessages(args, guildSettings, options, scope, ct);

        List<string> serverEmojis = args.Guild.Emojis.Values
            .Select(e => e.ToString())
            .OrderBy(_ => Guid.NewGuid())
            .Take(MaxEmojisInMessage)
            .ToList();

        var systemMessage =
            $"""
             - Твое имя "{options.BotName}". Не упоминай, что ты ИИ.
             - Ты должен отвечать в стиле пользователя из Discord. 
             - Старайся не упоминать людей в чате когда это не трубется (крайне редко пиши @Username)
             Вот примеры его сообщений:
             {string.Join("\n", userMessages.Select(m => $"- {m}").ToList())}

             - Иногда можешь использовать эмодзи сервера, но не злоупотребляй {string.Join(" ", serverEmojis)}
             - У тебя есть история сообщений, пытайся поддерживать диалог.
             """;

        return systemMessage;
    }

    private async Task<List<string>> GetImpersonationMessages(
        MessageCreatedEventArgs args,
        GuildChatSettings guildSettings,
        ImpersonationChatOptions chatOptions, 
        DbScope scope,
        CancellationToken ct)
    {
        IQueryable<MessageOrm> query = ChatService
            .GetQueryable(scope)
            .Where(x => x.GuildId == args.Guild.Id.ToString())
            .Where(x => x.Content.Length > MinContentLength)
            .Where(x => x.Content.Length <= MaxContentLength);

        if (guildSettings.ImpersonationUserId != null)
        {
            query = query.Where(x => x.UserId == guildSettings.ImpersonationUserId.ToString());
        }
        
        // TODO: Все сообщения грузятся в паямть.
        List<string> messages = await query.Select(x => x.Content).ToListAsync(ct);

        return FilterMessages(messages, chatOptions.ExampleMessagesTokenLimit).ToList();
    }

    private IEnumerable<string> FilterMessages(List<string> messages, int maxTokens)
    {
        int countToken = 0;
        foreach (var message in messages)
        {
            if (!MessageMatched(message))
            {
                continue;
            }

            countToken += message.Length;

            yield return message;

            if (countToken > maxTokens)
            {
                break;
            }
        }
    }

    private bool MessageMatched(string message)
    {
        // Меньше 3 слов.
        string[] words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 3)
        {
            return false;
        }
        
        if (UrlRegex.IsMatch(message) || EmojiRegex.IsMatch(message))
        {
            return false;
        }

        int mentionWordsCount = words.Count(x => ChatHelper.MentionRegex.IsMatch(x));
        if (mentionWordsCount > words.Length / 2)
        {
            return false;
        }

        int punctuationCount = message.Count(char.IsPunctuation);
        if (punctuationCount > message.Length * 0.3)
        {
            return false;
        }

        return true;
    }

    private static readonly Regex UrlRegex = new(@"https?://\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex EmojiRegex = new(@":[a-zA-Z0-9_]+:|[\u2190-\u21FF\u2600-\u27BF\uE000-\uF8FF]",
        RegexOptions.Compiled);

    record struct CacheValue(ulong? UserId, string SystemMessage);
}