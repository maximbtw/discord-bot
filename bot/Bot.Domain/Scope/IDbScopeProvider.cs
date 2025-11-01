using System.Data;

namespace Bot.Domain.Scope;

public interface IDbScopeProvider
{
    DbScope GetDbScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}