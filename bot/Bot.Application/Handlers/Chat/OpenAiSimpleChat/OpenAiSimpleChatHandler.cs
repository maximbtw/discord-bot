using System.ClientModel;
using Bot.Application.Infrastructure.Configuration;
using Bot.Contracts;
using Bot.Contracts.Handlers.AiChat;
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
        
        IEnumerable<ChatMessage> historyMessages = ChatHelper.LoadHistoryMessagesFromCache(
            _messageCache, 
            args.Guild.Id, 
            args.Channel.Id, 
            args.Message.Id, 
            _options.MaxInputTokenCount,
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
        
        await args.Message.RespondAsync(responseText);
    }
}