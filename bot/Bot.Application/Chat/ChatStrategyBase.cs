using System.ClientModel;
using Bot.Application.Chat.Services;
using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Chat;

internal abstract class ChatStrategyBase<TOptions> : IChatStrategy
{
    protected const int MessageContentMaxLength = 1000;
    
    protected readonly IChatService ChatService;
    
    private readonly ChatClient _client;
    private readonly ChatSettings _chatSettings;
    
    public ChatStrategyBase(
        IChatService chatService, 
        ChatClient client, 
        IConfiguration configuration)
    {
        ChatService = chatService;
        _client = client;

        _chatSettings = configuration.GetSection(nameof(ChatSettings)).Get<ChatSettings>()!;
    }
    
    public abstract ChatType Type { get; }
    
    protected abstract TOptions GetOptions(ChatSettings settings);
    
    public async Task Execute(
        DiscordClient client, 
        MessageCreatedEventArgs args,
        GuildChatSettings guildSettings, 
        CancellationToken ct)
    {
        var inputMessages = new List<ChatMessage>();

        List<SystemChatMessage> systemMessages = await CreateSystemChatMessages(client, args, guildSettings, ct);
        IEnumerable<ChatMessage> historyMessages = GetHistoryMessages(args, guildSettings);
        
        ChatMessage userMessage = new UserChatMessage(args.Message.Content)
        {
            ParticipantName = args.Author.Username,
        };
        
        inputMessages.AddRange(systemMessages);
        inputMessages.AddRange(historyMessages);
        inputMessages.Add(userMessage);
        
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _chatSettings.MaxOutputTokenCount,
            Temperature = _chatSettings.Temperature,
            TopP = _chatSettings.TopP,
            FrequencyPenalty = _chatSettings.FrequencyPenalty,
            PresencePenalty = _chatSettings.PresencePenalty,
        };
        
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(inputMessages, options, ct);

        string responseText = result.Value.Content[0].Text;
        
        if (guildSettings.ReplaceMentions)
        {
            responseText = await ChatHelper.ReplaceUserMentions(responseText, args.Guild, ct);
        }

        await args.Message.RespondAsync(responseText);
    }

    protected abstract ValueTask<List<SystemChatMessage>> CreateSystemChatMessages(
        DiscordClient client,
        MessageCreatedEventArgs args,
        GuildChatSettings guildSettings,
        CancellationToken ct);

    private IEnumerable<ChatMessage> GetHistoryMessages(MessageCreatedEventArgs args, GuildChatSettings guildSettings)
    {
        List<Message> cachedMessages = ChatService.GetMessagesFromCache(args.Guild.Id, args.Channel.Id)
            .Where(x => x.Id != args.Message.Id)
            .TakeLast(guildSettings.ChatHistoryLimit)
            .ToList();

        cachedMessages.Reverse();

        foreach (Message cachedMessage in cachedMessages)
        {
            if (cachedMessage.UserIsBot)
            {
                string content = ChatHelper.TruncateMessageContent(cachedMessage.Content, MessageContentMaxLength);

                yield return new AssistantChatMessage(content);
            }
            else
            {
                string content = ChatHelper.TruncateMessageContent(cachedMessage.Content, MessageContentMaxLength);

                yield return new UserChatMessage(content)
                {
                    ParticipantName = cachedMessage.UserNickname
                };
            }
        }
    }
    
    protected TOptions GetOptions() => GetOptions(_chatSettings);
}