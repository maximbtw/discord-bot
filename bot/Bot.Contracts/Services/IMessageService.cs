using Bot.Contracts.Shared;
using Bot.Domain.Message;
using Bot.Domain.Scope;

namespace Bot.Contracts.Services;

public interface IMessageService
{
    IQueryable<MessageOrm> GetQueryable(DbScope scope);
    
    Task Add(Message message, DbScope scope, CancellationToken ct, bool saveToCache = false);
    
    Task Add(List<Message> messages, DbScope scope, CancellationToken ct);
    
    Task DeleteGuildMessages(ulong guildId, List<ulong> channelIds, DbScope scope, CancellationToken ct);

    List<Message> GetMessagesFromCache(ulong guildId, ulong channelId);
}