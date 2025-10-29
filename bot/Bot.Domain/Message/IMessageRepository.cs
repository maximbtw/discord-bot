using Bot.Domain.Scope;

namespace Bot.Domain.Message;

public interface IMessageRepository
{
    Task BulkInsert(IEnumerable<MessageOrm> messages, DbScope scope, CancellationToken ct);
    
    Task Insert(MessageOrm message, DbScope scope, CancellationToken ct);

    IQueryable<MessageOrm> GetUpdateQueryable(DbScope scope);

    IQueryable<MessageOrm> GetQueryable(DbScope scope);
}