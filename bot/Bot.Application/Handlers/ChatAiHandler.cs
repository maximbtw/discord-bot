using Bot.Application.ChatAi;
using Bot.Application.Infrastructure.Configuration;
using Bot.Application.Shared;
using Bot.Contracts;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Bot.Application.Handlers;

internal class ChatAiHandler : IMessageCreatedHandler
{
    private readonly ChatAiResolver _resolver;
    private readonly ILogger<ChatAiHandler> _logger;
    private readonly BotConfiguration _configuration;

    public ChatAiHandler(
        ChatAiResolver resolver,
        ILogger<ChatAiHandler> logger,
        BotConfiguration configuration)
    {
        _resolver = resolver;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Execute(DiscordClient client, MessageCreatedEventArgs args)
    {
        if (!DiscordMessageHelper.MessageIsValid(args.Message, client.CurrentUser.Id))
        {
            return;
        }
        
        IChatAiStrategy strategy = _resolver.Resolve(_configuration.AiChatOptions.Strategy);
        
        var random = new Random();
        if (args.MentionedUsers.All(u => u.Id != client.CurrentUser.Id))
        {
            int roll = random.Next(0, 100);
            if (roll >= strategy.RandomMessageChance)
            {
                return;
            }
        }
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(strategy.TimeOutInSeconds));

        try
        {
            await strategy.Execute(client, args, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "AI response timeout reached.");
        }
    }
}