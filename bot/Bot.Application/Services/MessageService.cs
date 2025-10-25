using Bot.Contracts.Services;
using Bot.Contracts.Shared;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.Services;

internal class MessageService : IMessageService
{
    private readonly IMessageRepository _repository;

    public MessageService(IMessageRepository repository)
    {
        _repository = repository;
    }

    public IQueryable<MessageOrm> GetQueryable(DbScope scope)
    {
        return _repository.GetQueryable(scope);
    }

    public async Task Add(Message message, DbScope scope, CancellationToken ct = default)
    {
        MessageOrm orm = MapToOrm(message);

        await _repository.Insert(orm, scope, ct);
    }

    public async Task Add(List<Message> messages, DbScope scope, CancellationToken ct = default)
    {
        List<MessageOrm> orms = messages.ConvertAll(MapToOrm);

        if (scope.SupportTransaction)
        {
            await _repository.BulkInsert(orms, scope, ct);
        }
        else
        {
            foreach (MessageOrm orm in orms)
            {
                await _repository.Insert(orm, scope, ct);
            }
        }
    }

    public async Task DeleteGuildMessages(
        ulong guildId,
        List<ulong> channelIds,
        DbScope scope,
        CancellationToken ct = default)
    {
        IQueryable<MessageOrm> updateQueryable = _repository.GetUpdateQueryable(scope);

        IQueryable<MessageOrm> query = updateQueryable.Where(m => m.GuildId == guildId.ToString());
        if (channelIds.Any())
        {
            query = query.Where(m => channelIds.Any(y => y.ToString() == m.ChannelId));
        }

        await query.ExecuteDeleteAsync(ct);
    }

    private MessageOrm MapToOrm(Message message) => new()
    {
        Id = message.Id.ToString(),
        UserId = message.UserId.ToString(),
        UserNickname = message.UserNickname,
        UserIsBot = message.UserIsBot,
        ChannelId = message.ChannelId.ToString(),
        GuildId = message.GuildId.ToString(),
        Content = message.Content,
        Timestamp = message.Timestamp,
        ReplyToMessageId = message.ReplyToMessageId.ToString(),
        HasAttachments = message.HasAttachments,
        MentionedUserIds = message.MentionedUserIds.ConvertAll(x => x.ToString())
    };

    private Message MapToDto(MessageOrm orm) => new(
        ulong.Parse(orm.Id),
        ulong.Parse(orm.UserId),
        orm.UserNickname,
        orm.UserIsBot,
        ulong.Parse(orm.ChannelId),
        ulong.Parse(orm.GuildId),
        orm.Content,
        orm.Timestamp,
        orm.ReplyToMessageId == null ? null : ulong.Parse(orm.ReplyToMessageId),
        orm.HasAttachments,
        orm.MentionedUserIds.ConvertAll(x => ulong.Parse(x)));
}