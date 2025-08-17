using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Bot.Application.Shared;
using Bot.Contracts;
using Bot.Contracts.Ai.OpenRouter;
using Bot.Contracts.Shared;
using Bot.Domain.Message;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bot.Application.Handlers;

internal class AskAiHandler : IMessageCreatedHandler
{
    private const string Url = "https://openrouter.ai/api/v1/chat/completions";
    private static readonly HttpClient HttpClient = new();
    private readonly IMessageRepository _messageRepository;
    private readonly ICreatedMessageCache _messageCache;
    private readonly ILogger<AskAiHandler> _logger;
    private readonly string _apiKey = "";

    private const int MaxHistoryMessages = 30;
    private const int MaxMessageLength = 100;

    public AskAiHandler(
        IMessageRepository messageRepository, 
        ICreatedMessageCache messageCache, 
        ILogger<AskAiHandler> logger)
    {
        _messageRepository = messageRepository;
        _messageCache = messageCache;
        _logger = logger;
    }

    public ValueTask<bool> NeedExecute(DiscordClient client, MessageCreatedEventArgs args)
    {
        if (!DiscordMessageHelper.MessageIsValid(args.Message, client.CurrentUser.Id))
        {
            return new ValueTask<bool>(false);
        }

        if (args.MentionedUsers.All(u => u.Id != client.CurrentUser.Id))
        {
            return new ValueTask<bool>(false);
        }

        return new ValueTask<bool>(true);
    }

    public async Task Execute(DiscordClient client, MessageCreatedEventArgs args)
    {
        List<Message> messages = await CreateMessages(args);

        messages.Insert(0, new Message()
        {
            Role= "system",
            Content= "Ты друг пользователя (Джумпей). Сохраняй дух переписки, учитывай манеру пользователя. (Отвечай коротко, пытайся острить)"
        });
        
        messages.Add(new Message
        {
            Content = RemoveBotTagFromMessage(DiscordContentMapper.MapContent(args.Message), client.CurrentUser.Username),
            Role = "user",
            Name = args.Message.Author!.Username
        });

        var request = new ModelRequest
        {
            Model = "openai/gpt-oss-20b:free",
            Messages = messages,
            User = args.Message.Author!.Username
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        try
        {
            string json = JsonSerializer.Serialize(request, jsonOptions);
            _logger.LogInformation($"Send request: {json}");

            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Добавляем API-ключ
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            using HttpResponseMessage response = await HttpClient.PostAsync(Url, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("AI API returned error {StatusCode}: {Body}", response.StatusCode, errorBody);
                await args.Message.RespondAsync("Ошибка при обращении к AI-сервису 😕");
                return;
            }

            string responseContent = await response.Content.ReadAsStringAsync();

            ModelResponse? modelResponse;
            try
            {
                modelResponse = JsonSerializer.Deserialize<ModelResponse>(responseContent);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка десериализации ответа AI: {Response}", responseContent);
                await args.Message.RespondAsync("Не удалось обработать ответ AI 😕");
                return;
            }

            string? aiAnswer = modelResponse?.Choices.FirstOrDefault()?.Message.Content;
            if (string.IsNullOrWhiteSpace(aiAnswer))
            {
                _logger.LogWarning("AI вернул пустой ответ: {Response}", responseContent);
                aiAnswer = "AI пока не знает, что ответить 😅";
            }

            await args.Message.RespondAsync(aiAnswer);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при запросе к AI");
            await args.Message.RespondAsync("Не удалось подключиться к AI-сервису 😕");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при обработке AI");
            await args.Message.RespondAsync("Произошла непредвиденная ошибка 😕");
        }
    }

    private async ValueTask<List<Message>> CreateMessages(MessageCreatedEventArgs args)
    {
        List<ShortMessageInfo> cachedMessages = _messageCache.Get(args.Channel.Id, args.Author.Id);

        cachedMessages = cachedMessages.Where(x => x.Id != args.Message.Id).TakeLast(MaxHistoryMessages).ToList();
        cachedMessages.Reverse();

        if (cachedMessages.Count == MaxHistoryMessages)
        {
            return cachedMessages.ConvertAll(x => new Message
            {
                Content = Truncate(x.Content, MaxMessageLength),
                Name = x.UserName,
                Role = x.UserIsBot ? "assistant" : "user"
            });
        }

        DateTime lastMessageDateTime = cachedMessages.LastOrDefault()?.CreatedAt ?? DateTime.MaxValue;

        int needToLoad = MaxHistoryMessages - cachedMessages.Count;

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
            Content = Truncate(x.Content!, MaxMessageLength),
            Name = x.UserName,
            Role = x.UserIsBot ? "assistant" : "user"
        });
    }
    
    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Length <= maxLength) return value;
        
        int lastSpace = value.LastIndexOf(' ', maxLength);
        return lastSpace > 0 ? value[..lastSpace] : value[..maxLength];
    }

    private string RemoveBotTagFromMessage(string message, string botName)
    {
        return message.Replace($"@{botName}", string.Empty).Trim();
    }
}