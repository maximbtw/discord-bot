using Bot.Application.Shared;
using Bot.Contracts;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Bot.Application.Handlers;

internal class ChatAiHandler : IMessageCreatedHandler
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatAiHandler> _logger;

    public ChatAiHandler(IChatService chatService, ILogger<ChatAiHandler> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task Execute(DiscordClient client, MessageCreatedEventArgs args)
    {
        if (!DiscordMessageHelper.MessageIsValid(args.Message, client.CurrentUser.Id))
        {
            return;
        }

        var random = new Random();
        if (args.MentionedUsers.All(u => u.Id != client.CurrentUser.Id))
        {
            if (args.Author.IsBot)
            {
                return;
            }

            int roll = random.Next(0, 100);
            if (roll >= _chatService.RandomMessageChance)
            {
                return;
            }
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            await _chatService.HandleMessage(client, args, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "AI response timeout reached.");
        }
    }
}