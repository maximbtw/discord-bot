namespace Bot.Domain.Message;

public interface IMessageRepository
{
    Task BulkInsert(IEnumerable<MessageOrm> messages, CancellationToken ct = default);
    
    Task Insert(MessageOrm message, CancellationToken ct = default);
    
    Task DeleteServerMessages(long serverId, CancellationToken ct = default);

    IQueryable<MessageOrm> GetQueryable();
}