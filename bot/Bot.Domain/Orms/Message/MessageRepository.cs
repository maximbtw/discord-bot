using Bot.Domain.Scope;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.Orms.Message;

internal class MessageRepository : IMessageRepository
{
    public async Task BulkInsert(IEnumerable<MessageOrm> messages, DbScope scope, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(messages);

        DiscordDbContext context = scope.GetDbContext();
        
        await context.BulkInsertAsync(messages, cancellationToken: ct);
    }

    public async Task Insert(MessageOrm message, DbScope scope, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);
        
        DiscordDbContext context = scope.GetDbContext();

        await context.AddAsync(message, cancellationToken: ct);
    }

    public IQueryable<MessageOrm> GetUpdateQueryable(DbScope scope)
    {
        DiscordDbContext context = scope.GetDbContext();
        
        return context.Set<MessageOrm>();
    }        
    
    public IQueryable<MessageOrm> GetQueryable(DbScope scope)
    {
        DiscordDbContext context = scope.GetDbContext();
        
        return context.Set<MessageOrm>().AsNoTracking();
    }
}