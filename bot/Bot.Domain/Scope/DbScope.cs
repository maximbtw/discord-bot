using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bot.Domain.Scope;

public class DbScope : IAsyncDisposable
{
    private readonly DiscordDbContext _dbContext;
    private readonly IDbContextTransaction _transaction;
    private bool _committed;

    internal DbScope(DiscordDbContext dbContext, IsolationLevel isolationLevel)
    {
        _dbContext = dbContext;
        _transaction = _dbContext.Database.BeginTransaction(isolationLevel);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
        await _transaction.CommitAsync(ct);
        
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            await _transaction.RollbackAsync();   
        }

        await _transaction.DisposeAsync();
    }
}