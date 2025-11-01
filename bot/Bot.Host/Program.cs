using System.Threading.Tasks;
using Bot.Application.Chat;
using Bot.Application.Infrastructure.Configuration;
using Bot.Application.Jobs;
using Bot.Application.Services;
using Bot.Commands;
using Bot.Domain;
using Bot.Events;
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

var botConfiguration = config.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>()!;

var builder = DiscordClientBuilder.CreateDefault(
    botConfiguration.Token, 
    DiscordIntents.AllUnprivileged  | DiscordIntents.MessageContents | DiscordIntents.GuildVoiceStates | DiscordIntents.GuildMembers);

builder.ConfigureServices(x =>
{
    x.AddSingleton(loggerFactory);
    x.AddSingleton<IConfiguration>(config);
    x.AddLogging(); 
    
    x.RegisterDb(botConfiguration.DatabaseOptions);
    x.RegisterRepositories();
    x.RegisterServices();
    x.RegisterAiChat(config);
    x.RegisterJobs(config);

    x.AddMemoryCache();
});

builder.RegisterCommands(botConfiguration.Prefix);
builder.RegisterEvents();

DiscordClient client = builder.Build();

await client.ConnectAsync();
await Task.Delay(-1);
