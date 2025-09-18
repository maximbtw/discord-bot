using Bot.Domain.Scope;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.Message;

internal class MessageRepository : IMessageRepository
{
    public async Task BulkInsert(IEnumerable<MessageOrm> messages, DbScope scope, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(messages);

        DiscordDbContext context = scope.GetDbContext();
        
        await context.BulkInsertAsync(messages, cancellationToken: ct);
    }

    public async Task Insert(MessageOrm message, DbScope scope, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        
        DiscordDbContext context = scope.GetDbContext();

        await context.AddAsync(message, cancellationToken: ct);
    }

    public async Task DeleteServerMessages(long serverId, DbScope scope, CancellationToken ct)
    {
        DiscordDbContext context = scope.GetDbContext();
        
        await context.Set<MessageOrm>()
            .Where(m => m.ServerId == serverId)
            .ExecuteDeleteAsync(ct);
    }

    public IQueryable<MessageOrm> GetQueryable(DbScope scope)
    {
        DiscordDbContext context = scope.GetDbContext();
        
        return context.Set<MessageOrm>().AsNoTracking();
    }
}