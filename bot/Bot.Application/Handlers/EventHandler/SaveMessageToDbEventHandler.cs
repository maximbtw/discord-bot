using Bot.Application.Shared;
using Bot.Contracts;
using Bot.Contracts.Handlers;
using Bot.Contracts.Shared;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Bot.Application.Handlers.EventHandler;

internal class SaveMessageToDbEventHandler : IMessageCreatedEventHandler
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICreatedMessageCache _createdMessageCache;

    public SaveMessageToDbEventHandler(
        IMessageRepository messageRepository, 
        ICreatedMessageCache createdMessageCache)
    {
        _messageRepository = messageRepository;
        _createdMessageCache = createdMessageCache;
    }

    public async Task Execute(DiscordClient client, MessageCreatedEventArgs args, DbScope scope)
    {
        if (!DiscordMessageHelper.MessageIsValid(args.Message, client.CurrentUser.Id))
        {
            return;
        }

        bool isApplicationCommand = args.Message.Author!.Id == client.CurrentUser.Id &&
                                    args.Message.MessageType == DiscordMessageType.ApplicationCommand;

        if (isApplicationCommand)
        {
            return;
        }

        // Не сохранять ответы бота на команды.
        MessageDto message = DiscordContentMapper.MapDiscordMessageToDto(args.Message);

        _createdMessageCache.Add(args.Guild.Id, args.Channel.Id, message);

        await _messageRepository.Insert(DiscordContentMapper.MapDiscordMessage(args.Message), scope);

        await scope.CommitAsync();
    }
}