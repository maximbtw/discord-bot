using Bot.Contracts.Message;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Bot.Application.Services;

internal class MessageService : IMessageService
{
    private const int MaxMessagesInCache = 100;
    
    private readonly IMessageRepository _repository;
    private readonly IMemoryCache _memoryCache;

    public MessageService(IMessageRepository repository, IMemoryCache memoryCache)
    {
        _repository = repository;
        _memoryCache = memoryCache;
    }

    public IQueryable<MessageOrm> GetQueryable(DbScope scope)
    {
        return _repository.GetQueryable(scope);
    }

    public async Task Add(Message message, DbScope scope, CancellationToken ct = default, bool saveToCache = false)
    {
        MessageOrm orm = MapToOrm(message);

        await _repository.Insert(orm, scope, ct);

        if (saveToCache)
        {
            SaveToCache(message);
        }
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
    
    public List<Message> GetMessagesFromCache(ulong guildId, ulong channelId)
    {
        var key = new CacheKey(guildId, channelId);
        
        return _memoryCache.TryGetValue(key, out Queue<Message>? messages)
            ? messages!.ToList()
            : Enumerable.Empty<Message>().ToList();
    }

    public void ClearCache(ulong guildId, List<ulong> channelIds)
    {
        foreach (ulong channelId in channelIds)
        {
            var key = new CacheKey(guildId, channelId);
            
            _memoryCache.Remove(key);
        }
    }

    private void SaveToCache(Message message)
    {
        var key = new CacheKey(message.GuildId, message.ChannelId);
        
        Queue<Message> messages = _memoryCache.GetOrCreate(key, _ => new Queue<Message>())!;
        
        messages.Enqueue(message);
        
        if (messages.Count > MaxMessagesInCache)
        {
            messages.Dequeue();   
        }
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
        orm.MentionedUserIds.ConvertAll(ulong.Parse));
    
    private readonly record struct CacheKey(ulong GuildId, ulong ChannelId);
}