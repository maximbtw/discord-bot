using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Domain;

public class Migrator
{
    private readonly IServiceProvider _serviceProvider;

    public Migrator(string connectionString)
    {
        _serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .Configure<RunnerOptions>(options =>
            {
                options.Task = "migrate";
                options.Version = 0;
                options.Steps = 1;
            })
            .ConfigureRunner(runnerBuilder => runnerBuilder
                .AddPostgres15_0()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(Assembly.GetExecutingAssembly())
                .For.Migrations())
            .AddLogging(builder => builder.AddFluentMigratorConsole())
            .BuildServiceProvider();
    }

    public void MigrateUp()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        
        var migrationRunner = scope.ServiceProvider.GetService<IMigrationRunner>();
        
        migrationRunner!.MigrateUp();
    }
    
    public static void MigrateDatabase(string connectionString)
    {
        var migrator = new Migrator(connectionString);

        migrator.MigrateUp();
    }
}