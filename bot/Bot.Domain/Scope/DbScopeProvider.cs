using System.Data;

namespace Bot.Domain.Scope;

internal class DbScopeProvider : IDbScopeProvider
{
    private readonly DiscordDbContext _dbContext;
    
    public DbScopeProvider(DiscordDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public DbScope GetDbScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) =>
        new(_dbContext, useTransaction: true,  isolationLevel);

    public bool DatabaseIsRelation() => true;
}