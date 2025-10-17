using System.ClientModel;
using Bot.Application.Infrastructure.Configuration;
using Bot.Contracts;
using Bot.Contracts.Shared;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Ai;

internal class OpenAiChatService : IChatService
{
    private readonly ICreatedMessageCache _messageCache;
    private readonly ChatClient _client;
    private readonly OpenAiSettings _openAiSettings;

    public OpenAiChatService(
        ICreatedMessageCache messageCache,
        IConfiguration configuration,
        ChatClient client)
    {
        _messageCache = messageCache;
        _client = client;
        _openAiSettings = configuration.GetSection(nameof(OpenAiSettings)).Get<OpenAiSettings>()!;
    }

    public int RandomMessageChance => _openAiSettings.ChatOptions.RandomMessageChance;

    public async Task HandleMessage(DiscordClient discordClient, MessageCreatedEventArgs args, CancellationToken ct)
    {
        IEnumerable<ChatMessage> historyMessages = LoadHistoryMessagesFromCache(args);
        ChatMessage systemMessage = new SystemChatMessage(_openAiSettings.ChatOptions.SystemMessage);
        ChatMessage userMessage = new UserChatMessage(args.Message.Content)
        {
            ParticipantName = args.Author.Username,
        };
        
        var messages = new List<ChatMessage>();
        messages.Add(systemMessage);
        messages.AddRange(historyMessages);
        messages.Add(userMessage);

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _openAiSettings.ChatOptions.MaxOutputTokenCount,
            Temperature = 0.7f,       
            TopP = 0.9f,              
            FrequencyPenalty = 0.2f, 
            PresencePenalty = 0.3f
        };

        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages, options, ct);

        string responseText = result.Value.Content[0].Text;
        
        await args.Message.RespondAsync(responseText);
    }

    private IEnumerable<ChatMessage> LoadHistoryMessagesFromCache( MessageCreatedEventArgs args)
    {
        List<MessageDto> cachedMessages = _messageCache.GetLastMessages(args.Guild.Id, args.Channel.Id)
            .Where(x => x.Id != (long)args.Message.Id)
            .TakeLast(_openAiSettings.ChatOptions.MaxChatHistoryMessages)
            .ToList();

        cachedMessages.Reverse();

        foreach (MessageDto cachedMessage in cachedMessages)
        {
            if (cachedMessage.UserIsBot)
            {
                yield return new AssistantChatMessage(Truncate(cachedMessage.Content, _openAiSettings.ChatOptions.MaxInputTokenCount));
            }
            else
            {
                yield return new UserChatMessage(Truncate(cachedMessage.Content, _openAiSettings.ChatOptions.MaxInputTokenCount))
                {
                    ParticipantName = cachedMessage.UserName
                };
            }
        }
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