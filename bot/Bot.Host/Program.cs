using System.IO;
using System.Threading.Tasks;
using Bot.Application;
using Bot.Application.Infrastructure.Configuration;
using Bot.Domain;
using Bot.Host;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var configuration = config.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>()!;
ILoggerFactory loggerFactory = LoggingConfigurator.CreateLoggerFactory();

var services = new ServiceCollection();

services.AddSingleton(loggerFactory);
services.AddSingleton(configuration);

services.AddLogging(); 

services.RegisterDb(configuration.UseDb, configuration.DatabaseOptions?.ConnectionString);
services.RegisterRepositories();
services.RegisterUseCases(configuration);

services.AddMemoryCache();

var builder = DiscordClientBuilder.CreateDefault(
    configuration.Token, 
    DiscordIntents.AllUnprivileged  | DiscordIntents.MessageContents, services);

builder.RegisterCommands(configuration.Prefix);
builder.RegisterEvents(services);

DiscordClient client = builder.Build();

await client.ConnectAsync();
await Task.Delay(-1);
