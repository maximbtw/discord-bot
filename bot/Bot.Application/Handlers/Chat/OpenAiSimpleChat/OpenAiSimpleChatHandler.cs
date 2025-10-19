using System.ClientModel;
using Bot.Application.Infrastructure.Configuration;
using Bot.Contracts;
using Bot.Contracts.Handlers.AiChat;
using Bot.Contracts.Shared;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Handlers.Chat.OpenAiSimpleChat;

internal class OpenAiSimpleChatHandler : IAiChatHandler
{
    private readonly ICreatedMessageCache _messageCache;
    private readonly ChatClient _client;
    private readonly OpenAiSimpleChatOptions _options;

    public OpenAiSimpleChatHandler(
        ICreatedMessageCache messageCache,
        IConfiguration configuration,
        ChatClient client)
    {
        _messageCache = messageCache;
        _client = client;
        _options = configuration.GetSection(nameof(OpenAiSettings)).Get<OpenAiSettings>()!.ChatOptions.SimpleChatOptions;
    }

    public async Task HandleMessage(DiscordClient discordClient, MessageCreatedEventArgs args, DbScope scope, CancellationToken ct)
    {
        var inputMessages = new List<ChatMessage>();

        if (!string.IsNullOrEmpty(_options.SystemMessage))
        {
            inputMessages.Add(_options.SystemMessage);
        }
        
        IEnumerable<ChatMessage> historyMessages = LoadHistoryMessagesFromCache(args);
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
        
        await args.Message.RespondAsync(responseText);
    }

    private IEnumerable<ChatMessage> LoadHistoryMessagesFromCache(MessageCreatedEventArgs args)
    {
        List<MessageDto> cachedMessages = _messageCache.GetLastMessages(args.Guild.Id, args.Channel.Id)
            .Where(x => x.Id != (long)args.Message.Id)
            .TakeLast(_options.MaxChatHistoryMessages)
            .ToList();

        cachedMessages.Reverse();

        foreach (MessageDto cachedMessage in cachedMessages)
        {
            if (cachedMessage.UserIsBot)
            {
                yield return new AssistantChatMessage(Truncate(cachedMessage.Content, _options.MaxInputTokenCount));
            }
            else
            {
                yield return new UserChatMessage(Truncate(cachedMessage.Content, _options.MaxInputTokenCount))
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
}