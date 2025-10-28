using Bot.Application.Chat;
using Bot.Application.Infrastructure.Configuration;
using Bot.Application.Shared;
using Bot.Contracts.Services;
using Bot.Contracts.Shared;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Events;

internal class MessageCreatedEventHandler : IEventHandler<MessageCreatedEventArgs>
{
    private readonly IMessageService _messageService;
    private readonly IDbScopeProvider _dbScopeProvider;
    private readonly ChatStrategyResolver _chatStrategyResolver;
    private readonly ChatSettings _settings;
    private readonly ILogger<MessageCreatedEventHandler> _logger;

    public MessageCreatedEventHandler(
        IMessageService messageService,
        IDbScopeProvider dbScopeProvider, 
        ChatStrategyResolver chatStrategyResolver, 
        IConfiguration configuration, 
        ILogger<MessageCreatedEventHandler> logger)
    {
        _messageService = messageService;
        _dbScopeProvider = dbScopeProvider;
        _chatStrategyResolver = chatStrategyResolver;
        _logger = logger;
        _settings =  configuration.GetSection(nameof(ChatSettings)).Get<ChatSettings>()!;
    }
    
    public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
    {
        if (!DiscordMessageHelper.MessageIsValid(eventArgs.Message, sender.CurrentUser.Id))
        {
            return;
        }

        bool isApplicationCommand = eventArgs.Message.Author!.Id == sender.CurrentUser.Id &&
                                    eventArgs.Message.MessageType == DiscordMessageType.ApplicationCommand;

        // Не сохранять ответы бота на команды.
        if (isApplicationCommand)
        {
            return;
        }
        
        Message message = DiscordContentMapper.MapDiscordMessageToMessage(eventArgs.Message);

        await SaveMessage(message);
        await TryRespondToMessage(sender, eventArgs);
    }

    private async Task SaveMessage(Message message)
    {
        await using DbScope scope = _dbScopeProvider.GetDbScope();

        await _messageService.Add(message, scope, CancellationToken.None, saveToCache: true);

        await scope.CommitAsync();
    }

    private async Task TryRespondToMessage(DiscordClient sender, MessageCreatedEventArgs eventArg)
    {
        bool botMentioned = eventArg.MentionedUsers.Any(u => u.Id == sender.CurrentUser.Id);
        if (!botMentioned)
        {
            var random = new Random();
            if (eventArg.Author.IsBot)
            {
                return;
            }

            int roll = random.Next(0, 100);
            if (roll >= _settings.RandomMessageChance)
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
}