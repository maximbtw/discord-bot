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
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

ILoggerFactory loggerFactory = LoggingConfigurator.CreateLoggerFactory();

var services = new ServiceCollection();

services.AddSingleton(loggerFactory);
services.AddSingleton<IConfiguration>(config);  

services.AddLogging(); 

var botConfiguration = config.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>()!;

services.RegisterDb(botConfiguration.DatabaseOptions);
services.RegisterRepositories();
services.RegisterUseCases();
services.RegisterAiChat(config);

services.AddMemoryCache();

var builder = DiscordClientBuilder.CreateDefault(
    botConfiguration.Token, 
    DiscordIntents.AllUnprivileged  | DiscordIntents.MessageContents, services);

builder.RegisterCommands(botConfiguration.Prefix);
builder.RegisterEvents(services);

DiscordClient client = builder.Build();

await client.ConnectAsync();
await Task.Delay(-1);
