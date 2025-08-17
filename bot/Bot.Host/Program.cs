using System.IO;
using System.Threading.Tasks;
using Bot.Application;
using Bot.Application.Infrastructure.Configuration;
using Bot.Domain;
using Bot.Host;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

IConfigurationBuilder confBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = confBuilder.Build();

var configuration = config.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>()!;

var services = new ServiceCollection();

services.AddSingleton(configuration);

services.RegisterDb(configuration.DatabaseOptions.ConnectionString);
services.RegisterRepositories();
services.RegisterUseCases(configuration);

services.AddMemoryCache();

Migrator.MigrateDatabase(configuration.DatabaseOptions.ConnectionString);

var builder = DiscordClientBuilder.CreateDefault(
    configuration.Token, 
    DiscordIntents.AllUnprivileged  | DiscordIntents.MessageContents, services);

builder.RegisterCommands(configuration.Prefix);
builder.RegisterEvents(services);

DiscordClient client = builder.Build();

await client.ConnectAsync();
await Task.Delay(-1);
