using Bot.Application.Chat;
using Bot.Application.Chat.Services;
using Bot.Application.Shared;
using Bot.Contracts.Chat;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Events.MessageCreated;

internal class MessageCreatedEventHandler : IEventHandler<MessageCreatedEventArgs>
{
    private readonly IChatService _chatService;
    private readonly IDbScopeProvider _scopeProvider;
    private readonly ChatSettings _chatSettings;

    private readonly MessageCreatedEventChatHandler _chatHandler;

    public MessageCreatedEventHandler(
        IChatService chatService,
        IDbScopeProvider scopeProvider, 
        ChatStrategyResolver chatStrategyResolver, 
        IConfiguration configuration, 
        ILogger<MessageCreatedEventHandler> logger)
    {
        _chatService = chatService;
        _scopeProvider = scopeProvider;
        _chatSettings =  configuration.GetSection(nameof(ChatSettings)).Get<ChatSettings>()!;

        _chatHandler = new MessageCreatedEventChatHandler(
            scopeProvider,
            chatService,
            chatStrategyResolver,
            _chatSettings,
            logger);
    }
    
    public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
    {
        if (!ChatHelper.IsValidChatMessage(eventArgs.Message))
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
        await using DbScope scope = _scopeProvider.GetDbScope();
        
        Message message = DiscordContentMapper.MapDiscordMessageToMessage(eventArgs.Message);

        await _chatService.AddMessage(message, scope, CancellationToken.None, saveToCache: true);

        await scope.CommitAsync();
    }
}