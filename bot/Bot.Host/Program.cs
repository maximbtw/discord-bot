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
using Microsoft.AspNetCore.Builder;

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

// TODO: Нужен web сервис чтобы был healthcheck у джобы, иначе хостинг останавливает сервис
WebApplicationBuilder builderWeb = WebApplication.CreateBuilder();
WebApplication app = builderWeb.Build();
app.MapGet("/healthz", () => "OK");
_ = app.RunAsync(); 

await Task.Delay(-1);
