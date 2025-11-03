using Bot.Application.Chat;
using Bot.Application.Shared;
using Bot.Contracts.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Events.MessageCreated;

internal class MessageCreatedEventHandler : IEventHandler<MessageCreatedEventArgs>
{
    private readonly IMessageService _messageService;
    private readonly IDbScopeProvider _dbScopeProvider;
    private readonly ChatSettings _chatSettings;

    private readonly MessageCreatedEventChatHandler _chatHandler;

    public MessageCreatedEventHandler(
        IMessageService messageService,
        IDbScopeProvider dbScopeProvider, 
        ChatStrategyResolver chatStrategyResolver, 
        IConfiguration configuration, 
        ILogger<MessageCreatedEventHandler> logger)
    {
        _messageService = messageService;
        _dbScopeProvider = dbScopeProvider;
        _chatSettings =  configuration.GetSection(nameof(ChatSettings)).Get<ChatSettings>()!;
        
        _chatHandler = new MessageCreatedEventChatHandler(dbScopeProvider, chatStrategyResolver, _chatSettings, logger);
    }
    
    public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
    {
        if (ChatHelper.IsValidChatMessage(eventArgs.Message))
        {
            return;
        }
        
        await SaveMessage(eventArgs);

        if (_chatSettings.Enabled)
        {
            await _chatHandler.Execute(sender, eventArgs);   
        }
    }

    private async Task SaveMessage(MessageCreatedEventArgs eventArgs)
    {
        await using DbScope scope = _dbScopeProvider.GetDbScope();
        
        Message message = DiscordContentMapper.MapDiscordMessageToMessage(eventArgs.Message);

        await _messageService.Add(message, scope, CancellationToken.None, saveToCache: true);

        await scope.CommitAsync();
    }
}