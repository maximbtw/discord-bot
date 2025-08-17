using Bot.Application.Shared;
using Bot.Contracts;
using Bot.Contracts.Shared;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot.Application.Handlers;

internal class SaveMessageToDbHandler : IMessageCreatedHandler
{
    private readonly IDbScopeProvider _scopeProvider;
    private readonly IMessageRepository _messageRepository;
    private readonly ICreatedMessageCache _createdMessageCache;
    
    public SaveMessageToDbHandler(IMessageRepository messageRepository, IDbScopeProvider scopeProvider, ICreatedMessageCache createdMessageCache)
    {
        _messageRepository = messageRepository;
        _scopeProvider = scopeProvider;
        _createdMessageCache = createdMessageCache;
    }

    public ValueTask<bool> NeedExecute(DiscordClient client, MessageCreatedEventArgs args)
    {
        return new ValueTask<bool>(DiscordMessageHelper.MessageIsValid(args.Message, client.CurrentUser.Id));
    }

    public async Task Execute(DiscordClient client, MessageCreatedEventArgs args)
    {
        var info = new ShortMessageInfo(
            args.Message.Id,
            DiscordContentMapper.MapContent(args.Message),
            args.Message.Author!.Id, 
            args.Message.Author.Username,
            args.Message.Author.IsBot,
            args.Message.Timestamp.UtcDateTime);
        
        _createdMessageCache.Add(args.Guild.Id, args.Channel.Id, info);
        
        await using DbScope scope = _scopeProvider.GetDbScope();

        await _messageRepository.Insert(DiscordContentMapper.MapDiscordMessage(args.Message));

        await scope.CommitAsync();
    }
}