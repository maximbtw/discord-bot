using Bot.Application.Infrastructure.Configuration.AiChat;
using Bot.Application.Shared;
using Bot.Contracts;
using Bot.Contracts.ChatAi.OpenRouter;
using Bot.Contracts.Shared;
using Bot.Domain.Message;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bot.Application.ChatAi.OpenRouter;

internal class ChatAiOpenRouterStrategy : IChatAiStrategy
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICreatedMessageCache _messageCache;
    private readonly AiChatOpenRouterSettings _settings;
    private readonly ILogger<IChatAiStrategy> _logger;
    private readonly ChatAiOpenRouterClient _client;

    public ChatAiOpenRouterStrategy(
        IMessageRepository messageRepository,
        ICreatedMessageCache messageCache,
        AiChatOpenRouterSettings settings,
        ILogger<IChatAiStrategy> logger,
        ChatAiOpenRouterClient client)
    {
        _messageRepository = messageRepository;
        _messageCache = messageCache;
        _settings = settings;
        _logger = logger;
        _client = client;
    }

    public AiChatStrategy StrategyName => AiChatStrategy.OpenRouter;
    
    public int TimeOutInSeconds => _settings.TimeOutInSeconds;
    
    public int RandomMessageChance => _settings.RandomMessageChance;

    public async Task Execute(DiscordClient client, MessageCreatedEventArgs args, CancellationToken ct)
    {
        List<Message> messages = await CreateMessages(args);

        if (!string.IsNullOrEmpty(_settings.SystemMessage))
        {
            messages.Insert(0, new Message
            {
                Role= "system",
                Content= _settings.SystemMessage
            });   
        }
        
        messages.Add(new Message
        {
            Content = RemoveBotTagFromMessage(DiscordContentMapper.MapContent(args.Message), client.CurrentUser.Username),
            Role = "user",
            Name = args.Message.Author!.Username
        });

        var request = new ModelRequest
        {
            Model = _settings.Model,
            Messages = messages,
            User = args.Message.Author!.Username
        };

        try
        {
            ModelResponse? response = await _client.PostAsync(request, ct);

            string? text = response?.Choices.FirstOrDefault()?.Message.Content;

            string answer = string.IsNullOrEmpty(text) ? _settings.BadRequestMessage : text;
            
            await args.Message.RespondAsync(answer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            
            await args.Message.RespondAsync(_settings.BadRequestMessage);
        }
    }

    private async ValueTask<List<Message>> CreateMessages(MessageCreatedEventArgs args)
    {
        List<ShortMessageInfo> cachedMessages = _messageCache.Get(args.Channel.Id, args.Author.Id);

        cachedMessages = cachedMessages.Where(x => x.Id != args.Message.Id).TakeLast(_settings.MaxChatHistoryMessages)
            .ToList();
        cachedMessages.Reverse();

        if (cachedMessages.Count == _settings.MaxChatHistoryMessages)
        {
            return cachedMessages.ConvertAll(x => new Message
            {
                Content = Truncate(x.Content, _settings.MaxMessageLength),
                Name = x.UserName,
                Role = x.UserIsBot ? "assistant" : "user"
            });
        }

        DateTime lastMessageDateTime = cachedMessages.LastOrDefault()?.CreatedAt ?? DateTime.MaxValue;

        int needToLoad = _settings.MaxChatHistoryMessages - cachedMessages.Count;

        List<MessageOrm> messagesFromDb = await _messageRepository
            .GetQueryable()
            .Where(x => x.Id != (long)args.Message.Id)
            .Where(x => x.ServerId == (long)args.Channel.GuildId! && x.ChannelId == (long)args.Channel.Id)
            .Where(x => x.Timestamp < lastMessageDateTime)
            .OrderByDescending(x => x.Timestamp)
            .Take(needToLoad)
            .ToListAsync();

        messagesFromDb.Reverse();

        return messagesFromDb.ConvertAll(x => new Message
        {
            Content = Truncate(x.Content!, _settings.MaxMessageLength),
            Name = x.UserName,
            Role = x.UserIsBot ? "assistant" : "user"
        });
    }

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

    private string RemoveBotTagFromMessage(string message, string botName)
    {
        return message.Replace($"@{botName}", string.Empty).Trim();
    }
}