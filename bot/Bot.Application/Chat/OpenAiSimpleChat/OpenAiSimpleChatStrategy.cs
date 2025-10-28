using System.ClientModel;
using Bot.Contracts.Services;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Chat.OpenAiSimpleChat;

internal class OpenAiSimpleChatStrategy : IChatStrategy
{
    private readonly IMessageService _messageService;
    private readonly ChatClient _client;
    private readonly OpenAiSimpleChatOptions _chatOptions;

    public ChatType Type => ChatType.SimpleChat;

    public OpenAiSimpleChatStrategy(
        IMessageService messageService, 
        ChatClient client,
        IConfiguration configuration)
    {
        _messageService = messageService;
        _client = client;
        _chatOptions = configuration.GetSection(nameof(ChatSettings)).Get<ChatSettings>()!.SimpleChatOptions;
    }

    public async Task Execute(
        DiscordClient discordClient, 
        MessageCreatedEventArgs args, 
        DbScope scope,
        CancellationToken ct)
    {
        var inputMessages = new List<ChatMessage>();

        if (!string.IsNullOrEmpty(_chatOptions.SystemMessage))
        {
            inputMessages.Add(_chatOptions.SystemMessage);
        }

        IEnumerable<ChatMessage> historyMessages = ChatHelper.LoadHistoryMessagesFromCache(
            _messageService,
            args.Guild.Id,
            args.Channel.Id,
            args.Message.Id,
            _chatOptions.MaxInputTokenCount,
            _chatOptions.MaxChatHistoryMessages);

        inputMessages.AddRange(historyMessages);

        ChatMessage userMessage = new UserChatMessage(args.Message.Content)
        {
            ParticipantName = args.Author.Username,
        };

        inputMessages.Add(userMessage);

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _chatOptions.MaxOutputTokenCount,
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