using Bot.Application.Infrastructure.Configuration;
using Bot.Application.Shared;
using Bot.Contracts.Handlers;
using Bot.Contracts.Handlers.AiChat;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Application.Handlers.EventHandler;

internal class SendMessageToOpenAiEventHandler : IMessageCreatedEventHandler
{
    private readonly IAiChatHandler _aiChatHandler;
    private readonly ILogger<SendMessageToOpenAiEventHandler> _logger;
    private readonly ChatOptions _options;
    

    public SendMessageToOpenAiEventHandler(
        IAiChatHandler aiChatHandler, 
        IConfiguration configuration,
        ILogger<SendMessageToOpenAiEventHandler> logger)
    {
        _aiChatHandler = aiChatHandler;
        _options =  configuration.GetSection(nameof(OpenAiSettings)).Get<OpenAiSettings>()!.ChatOptions;
        _logger = logger;
    }

    public async Task Execute(DiscordClient client, MessageCreatedEventArgs args, DbScope scope)
    {
        if (!DiscordMessageHelper.MessageIsValid(args.Message, client.CurrentUser.Id))
        {
            return;
        }

        // Не отвечать самому себе.
        if (args.Author.Id == client.CurrentUser.Id)
        {
            return;
        }

        bool botMentioned = args.MentionedUsers.Any(u => u.Id == client.CurrentUser.Id);
        if (!botMentioned)
        {
            var random = new Random();
            if (args.Author.IsBot)
            {
                return;
            }

            int roll = random.Next(0, 100);
            if (roll >= _options.RandomMessageChance)
            {
                return;
            }  
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeOutInSeconds));

        try
        {
            await args.Channel.TriggerTypingAsync();
            
            await _aiChatHandler.HandleMessage(client, args, scope, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "AI response timeout reached.");
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(_options.BadRequestMessage) && botMentioned)
            {
                await args.Message.RespondAsync(_options.BadRequestMessage);
            }
            
            _logger.LogError(ex, "Unexpected error.");
        }
    }
}