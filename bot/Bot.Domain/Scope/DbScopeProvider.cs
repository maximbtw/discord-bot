using System.Data;
using Microsoft.Extensions.Logging;

namespace Bot.Domain.Scope;

internal class DbScopeProvider : IDbScopeProvider
{
    private readonly DiscordDbContext _dbContext;
    private readonly ILogger<DbScopeProvider> _logger;
    
    public DbScopeProvider(DiscordDbContext dbContext, ILogger<DbScopeProvider> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public DbScope GetDbScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int retryCount = 3)
    {
        var attempts = 0;
        TimeSpan delay = TimeSpan.FromMilliseconds(500);

        while (true)
        {
            try
            {
                attempts++;
                return new DbScope(_dbContext, useTransaction: true, isolationLevel);
            }
            catch (Exception ex)
            {
                if (attempts >= retryCount)
                {
                    _logger.LogError(ex, 
                        "Max retry count {MaxRetries} reached. Failed to create DbScope.", retryCount);
                    throw;
                }

                Thread.Sleep(delay); 
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); 
            }
        }
    }
}