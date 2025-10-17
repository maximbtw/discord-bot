using System.Reflection;
using Bot.Domain.Configuration;
using Bot.Domain.Scope;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Domain;

public static class DependencyInjectionExtensions
{
    public static void RegisterRepositories(this IServiceCollection services)
    {
        const string repositorySuffix = "Repository";
        
        IEnumerable<Type> repositories =  Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => 
                t is { IsClass: true, IsAbstract: false } &&
                t.GetInterfaces().Any(i => i.Name.EndsWith(repositorySuffix)));

        foreach (Type repoType in repositories)
        {
            Type? interfaceType = repoType.GetInterfaces()
                .FirstOrDefault(i => i.Name.EndsWith(repositorySuffix));

            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, repoType);
            }
        }
    }
    
    public static void RegisterDb(this IServiceCollection services, DatabaseOptions databaseOptions)
    {
        if (databaseOptions.UseDb)
        {
            services.AddDbContext<DiscordDbContext>(options => options.UseNpgsql(databaseOptions.ConnectionString));

            services.AddScoped<IDbScopeProvider, DbScopeProvider>();
        
            Migrator.MigrateDatabase(databaseOptions.ConnectionString!);    
        }
        else
        {
            services.AddScoped<IDbScopeProvider, NoDbScopeProvider>();
        }
    }
}