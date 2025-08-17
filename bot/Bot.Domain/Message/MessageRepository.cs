using Bot.Domain.Scope;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.Message;

internal class MessageRepository : IMessageRepository
{
    private readonly DiscordDbContext _dbContext;
    
    public MessageRepository(DiscordDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task BulkInsert(IEnumerable<MessageOrm> messages, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(messages);

        await _dbContext.BulkInsertAsync(messages, cancellationToken: ct);
    }

    public async Task Insert(MessageOrm message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await _dbContext.AddAsync(message, cancellationToken: ct);
    }

    public async Task DeleteServerMessages(long serverId, CancellationToken ct)
    {
        await _dbContext.Set<MessageOrm>()
            .Where(m => m.ServerId == serverId)
            .ExecuteDeleteAsync(ct);
    }

    public IQueryable<MessageOrm> GetQueryable()
    {
        return _dbContext.Set<MessageOrm>().AsNoTracking();
    }
}