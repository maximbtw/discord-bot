using Bot.Application.Chat;
using Bot.Contracts.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Bot.Events.MessageCreated;

internal class MessageCreatedEventChatHandler
{
    private readonly IDbScopeProvider _dbScopeProvider;
    private readonly ChatStrategyResolver _chatStrategyResolver;
    private readonly ChatSettings _settings;
    private readonly ILogger<MessageCreatedEventHandler> _logger;

    internal MessageCreatedEventChatHandler(
        IDbScopeProvider dbScopeProvider,
        ChatStrategyResolver chatStrategyResolver,
        ChatSettings settings, 
        ILogger<MessageCreatedEventHandler> logger)
    {
        _dbScopeProvider = dbScopeProvider;
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
        bool botMentioned = eventArg.MentionedUsers.Any(u => u.Id == sender.CurrentUser.Id);
        if (!botMentioned)
        {
            if (!ShouldRespondByChance())
            {
                return;
            }
        }
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeOutInSeconds));

        try
        {
            await eventArg.Channel.TriggerTypingAsync();

            IChatStrategy chat = _chatStrategyResolver.Resolve(_settings.DefaultStrategy);
            
            await using DbScope scope = _dbScopeProvider.GetDbScope();
            
            await chat.Execute(sender, eventArg, scope, cts.Token);
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
    
    private bool ShouldRespondByChance()
    {
        var random = new Random();
        int roll = random.Next(0, 100);
        
        return roll < _settings.RandomMessageChance;
    }
}