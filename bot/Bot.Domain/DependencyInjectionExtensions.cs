using System.Reflection;
using Bot.Domain.Configuration;
using Bot.Domain.Orms;
using Bot.Domain.Orms.Message;
using Bot.Domain.Scope;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Domain;

public static class DependencyInjectionExtensions
{
    private const string RepositorySuffix = "Repository";
    
    public static void RegisterRepositories(this IServiceCollection services)
    {
        IEnumerable<Type> repositories =  Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => 
                t is { IsClass: true, IsAbstract: false } &&
                t.GetInterfaces().Any(i => i.Name.EndsWith(RepositorySuffix)));

        foreach (Type repoType in repositories)
        {
            Type? interfaceType = repoType.GetInterfaces()
                .FirstOrDefault(i => i.Name.EndsWith(RepositorySuffix));

            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, repoType);
            }
        }
    }
    
    public static void RegisterDb(this IServiceCollection services, DatabaseOptions databaseOptions)
    {
        if (!databaseOptions.UseInMemoryDatabase)
        {
            services.AddDbContext<DiscordDbContext>(options => options.UseNpgsql(databaseOptions.ConnectionString));

            services.AddScoped<IDbScopeProvider, DbScopeProvider>();
        
            Migrator.MigrateDatabase(databaseOptions.ConnectionString!);    
        }
        else
        {
            services.AddDbContext<DiscordDbContext>(options =>
                options.UseInMemoryDatabase("DiscordBotMemory"));

            services.AddScoped<IDbScopeProvider, InMemoryDbScopeProvider>();
        }
    }
}