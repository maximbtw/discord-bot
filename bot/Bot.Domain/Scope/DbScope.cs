using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bot.Domain.Scope;

public class DbScope : IAsyncDisposable
{
    private readonly DiscordDbContext _dbContext;
    private readonly IDbContextTransaction? _transaction;
    private bool _committed;

    internal DbScope(
        DiscordDbContext dbContext, 
        bool useTransaction = true, 
        IsolationLevel isolationLevel = IsolationLevel.Snapshot)
    {
        _dbContext = dbContext;

        if (useTransaction)
        {
            _transaction = _dbContext.Database.BeginTransaction(isolationLevel);   
        }
    }
    
    internal DiscordDbContext GetDbContext() => _dbContext;

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);   
        }
        
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            if (!_committed)
            {
                await _transaction.RollbackAsync();   
            }

            await _transaction.DisposeAsync();   
        }
    }
}