﻿using System.Data;

namespace Bot.Domain.Scope;

internal class InMemoryDbScopeProvider : IDbScopeProvider
{
    private readonly DiscordDbContext _context;

    public InMemoryDbScopeProvider(DiscordDbContext context)
    {
        _context = context;
    }

    public DbScope GetDbScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return new DbScope(_context, useTransaction: false);
    }
}