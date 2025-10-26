using Bot.Application.Shared;
using Bot.Contracts.Handlers;
using Bot.Contracts.Services;
using Bot.Contracts.Shared;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Bot.Application.Handlers.EventHandler;

internal class SaveMessageToDbEventHandler : IMessageCreatedEventHandler
{
    private readonly IMessageService _messageService;

    public SaveMessageToDbEventHandler(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task Execute(DiscordClient client, MessageCreatedEventArgs args, DbScope scope)
    {
        if (!DiscordMessageHelper.MessageIsValid(args.Message, client.CurrentUser.Id))
        {
            return;
        }

        bool isApplicationCommand = args.Message.Author!.Id == client.CurrentUser.Id &&
                                    args.Message.MessageType == DiscordMessageType.ApplicationCommand;

        // Не сохранять ответы бота на команды.
        if (isApplicationCommand)
        {
            return;
        }
        
        Message message = DiscordContentMapper.MapDiscordMessageToMessage(args.Message);

        await _messageService.Add(message, scope, CancellationToken.None, saveToCache: true);

        await scope.CommitAsync();
    }
}