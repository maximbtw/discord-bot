using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bot.Domain.Scope;

internal class NoDbScopeProvider : IDbScopeProvider
{
    public DbScope GetDbScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return new DbScope(new DiscordDbContext(new DbContextOptions<DiscordDbContext>(), new LoggerFactory()));
    }
}