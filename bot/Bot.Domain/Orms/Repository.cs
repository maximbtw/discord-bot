using Bot.Domain.Scope;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.Orms;

internal class Repository<TOrm> where TOrm : class, IOrm
{
    public async Task BulkInsert(IEnumerable<TOrm> orms, DbScope scope, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orms);

        DiscordDbContext context = scope.GetDbContext();
        
        await context.BulkInsertAsync(orms, cancellationToken: ct);
    }

    public async Task Insert(TOrm orm, DbScope scope, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orm);
        
        DiscordDbContext context = scope.GetDbContext();

        await context.AddAsync(orm, cancellationToken: ct);
    }

    public IQueryable<TOrm> GetUpdateQueryable(DbScope scope)
    {
        DiscordDbContext context = scope.GetDbContext();
        
        return context.Set<TOrm>();
    }  
    
    public IQueryable<TOrm> GetQueryable(DbScope scope)
    {
        DiscordDbContext context = scope.GetDbContext();
        
        return context.Set<TOrm>().AsNoTracking();
    }
}