using System.Data;

namespace Bot.Domain.Scope;

internal class NoDbScopeProvider : IDbScopeProvider
{
    public DbScope GetDbScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        throw new NotSupportedException();
    }
}