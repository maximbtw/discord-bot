using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bot.Application.Infrastructure.Checks.Access;
using Bot.Application.Infrastructure.Configuration;
using Bot.Contracts;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Host;

public static class DependencyInjectionExtensions
{
    public static void RegisterCommands(this DiscordClientBuilder builder, string prefix)
    { 
        builder.UseCommands((_, extension) =>
        {
            IEnumerable<Type> commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false } &&
                    typeof(DiscordCommandsGroupBase).IsAssignableFrom(t));
            
            foreach (Type type in commandTypes)
            {
                extension.AddCommands(type);
            }
    
            TextCommandProcessor textCommandProcessor = new(new TextCommandConfiguration
            {
                PrefixResolver = new DefaultPrefixResolver(allowMention: true, prefix).ResolvePrefixAsync,
            });
    
            extension.AddProcessor(textCommandProcessor);

            extension.AddCheck<RoleCheck>();
        });
    }
    
    public static void RegisterEvents(this DiscordClientBuilder builder)
    {
        builder.ConfigureEventHandlers(x =>
        {
            x.HandleMessageCreated((client, args) =>
            {
                IEnumerable<IMessageCreatedHandler> handlers = client.ServiceProvider.GetServices<IMessageCreatedHandler>();
                IEnumerable<Task> tasks = handlers.Select(async handler =>
                {
                    await handler.Execute(client, args);
                });

                _ = Task.WhenAll(tasks);
                
                return Task.CompletedTask;
            });
        });
    }
}