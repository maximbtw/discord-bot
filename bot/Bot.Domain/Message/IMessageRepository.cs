using Bot.Domain.Scope;

namespace Bot.Domain.Message;

public interface IMessageRepository
{
    Task BulkInsert(IEnumerable<MessageOrm> messages, DbScope scope, CancellationToken ct = default);
    
    Task Insert(MessageOrm message, DbScope scope, CancellationToken ct = default);
    
    Task DeleteServerMessages(long serverId, DbScope scope, CancellationToken ct = default);

    IQueryable<MessageOrm> GetQueryable(DbScope scope);
}