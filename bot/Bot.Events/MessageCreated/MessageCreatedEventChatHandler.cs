using Bot.Application.Chat;
using Bot.Application.Chat.Services;
using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Bot.Events.MessageCreated;

internal class MessageCreatedEventChatHandler
{
    private readonly IDbScopeProvider _dbScopeProvider;
    private readonly IChatService _chatService;
    private readonly ChatStrategyResolver _chatStrategyResolver;
    private readonly ChatSettings _settings;
    private readonly ILogger<MessageCreatedEventHandler> _logger;

    internal MessageCreatedEventChatHandler(
        IDbScopeProvider dbScopeProvider, 
        IChatService chatService,
        ChatStrategyResolver chatStrategyResolver,
        ChatSettings settings, 
        ILogger<MessageCreatedEventHandler> logger)
    {
        _dbScopeProvider = dbScopeProvider;
        _chatService = chatService;
        _chatStrategyResolver = chatStrategyResolver;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Chat logic:
    /// 1. The bot responds to any message with a certain probability defined by RandomMessageChance.
    /// 2. The bot always responds when it is mentioned in a message.
    /// </summary>
    internal async Task Execute(DiscordClient sender, MessageCreatedEventArgs eventArg)
    {
        GuildChatSettings guildSettings = await GetGuildSettings(eventArg.Guild.Id);
        
        bool botMentioned = eventArg.MentionedUsers.Any(u => u.Id == sender.CurrentUser.Id);
        if (!NeedToExecute(sender, eventArg, guildSettings, botMentioned))
        {
            return;
        }
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeOutInSeconds));
        try
        {
            await eventArg.Channel.TriggerTypingAsync();

            IChatStrategy chat = _chatStrategyResolver.Resolve(guildSettings.ChatType);
            
            await chat.Execute(sender, eventArg, guildSettings, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "AI response timeout reached");
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(_settings.BadRequestMessage) && botMentioned)
            {
                await eventArg.Message.RespondAsync(_settings.BadRequestMessage);
            }
            
            _logger.LogError(ex, "Unexpected error");
        }
    }

    private async Task<GuildChatSettings> GetGuildSettings(ulong guildId)
    {
        await using DbScope scope = _dbScopeProvider.GetDbScope();

        return await _chatService.GetGuildSettings(guildId, scope);
    }

    private bool NeedToExecute(
        DiscordClient sender,
        MessageCreatedEventArgs eventArg, 
        GuildChatSettings guildSettings,
        bool botMentioned)
    {
        if (eventArg.Message.Author!.Id == sender.CurrentUser.Id)
        {
            return false;
        }

        return botMentioned || ShouldRespondByChance(guildSettings.ResponseChance);
    }

    private bool ShouldRespondByChance(int responseChance)
    {
        var random = new Random();
        int roll = random.Next(0, 100);
        
        return roll < responseChance;
    }
}