using System.Text.RegularExpressions;
using Bot.Application.Chat.Services;
using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using Bot.Domain.Orms.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Chat.ImpersonationChat;

internal class ImpersonationChatStrategy  : ChatStrategyBase<ImpersonationChatOptions>
{
    private readonly IDbScopeProvider _scopeProvider;
    
    public ImpersonationChatStrategy(
        IConfiguration configuration, 
        ChatClient client,
        IChatService chatService, IDbScopeProvider scopeProvider) : base(chatService, client, configuration)
    {
        _scopeProvider = scopeProvider;
    }
    
    public override ChatType Type => ChatType.Impersonation;

    protected override ImpersonationChatOptions GetOptions(ChatSettings settings) => settings.ImpersonationChatOptions;

    protected override async ValueTask<List<SystemChatMessage>> CreateSystemChatMessages(
        DiscordClient client, 
        MessageCreatedEventArgs args, 
        GuildChatSettings guildSettings,
        CancellationToken ct)
    {
        var messages = new List<SystemChatMessage>();
        
        ImpersonationChatOptions options = GetOptions();

        await using DbScope scope = _scopeProvider.GetDbScope();
        
        List<MessageOrm> impersonationMessages = GetImpersonationMessages(args, guildSettings, options, scope);
        List<string> serverEmojis = args.Guild.Emojis.Values.Select(e => e.ToString()).ToList();

        var systemMessage =
            $"""
             - Твое имя "{options.BotName}". Не упоминай, что ты ИИ.
             - Ты должен отвечать в стиле пользователя из Discord. 
             - Старайся не упоминать людей в чате когда это не трубется (крайне редко пиши @Username)
             Вот примеры его сообщений:
             {string.Join("\n", impersonationMessages.Select(m => $"- {m.Content}").ToList())}

             - Иногда можешь использовать эмодзи сервера, но не злоупотребляй {string.Join(" ", serverEmojis)}
             - У тебя есть история сообщений, пытайся поддерживать диалог.
             """;
        
        messages.Add(new SystemChatMessage(systemMessage));
        
        return messages;
    }

    private List<MessageOrm> GetImpersonationMessages(
        MessageCreatedEventArgs args,
        GuildChatSettings guildSettings,
        ImpersonationChatOptions chatOptions, 
        DbScope scope)
    {
        var impersonationMessages = new List<MessageOrm>();

        IEnumerable<MessageOrm> messages = ChatService
            .GetQueryable(scope)
            .Where(x => x.GuildId == args.Guild.Id.ToString())
            .OrderByDescending(x => x.Timestamp);

        if (guildSettings.ImpersonationUserId != null)
        {
            messages = messages.Where(x => x.UserId == guildSettings.ImpersonationUserId.ToString());
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

            if (countToken > chatOptions.ExampleMessagesTokenLimit)
            {
                break;
            }
        }

        return impersonationMessages;
    }

    private bool MessageMatched(MessageOrm message)
    {
        // Слишком короткие и слишком длинные сообещния.
        if (message.Content.Length is < 15 or > 300)
        {
            return false;
        }

        // Меньше 3 слов.
        string[] words = message.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 3)
        {
            return false;
        }
        
        if (UrlRegex.IsMatch(message.Content) || EmojiRegex.IsMatch(message.Content))
        {
            return false;
        }

        int mentionWordsCount = words.Count(x => ChatHelper.MentionRegex.IsMatch(x));
        if (mentionWordsCount > words.Length / 2)
        {
            return false;
        }

        int punctuationCount = message.Content.Count(char.IsPunctuation);
        if (punctuationCount > message.Content.Length * 0.3)
        {
            return false;
        }

        return true;
    }

    private static readonly Regex UrlRegex = new(@"https?://\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex EmojiRegex = new(@":[a-zA-Z0-9_]+:|[\u2190-\u21FF\u2600-\u27BF\uE000-\uF8FF]",
        RegexOptions.Compiled);
    
}