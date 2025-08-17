using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.Scope;

internal class DiscordDbContext : DbContext
{
    public DiscordDbContext(DbContextOptions<DiscordDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}