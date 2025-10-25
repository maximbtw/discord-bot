using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bot.Application.Infrastructure.Configuration;
using Bot.Commands;
using Bot.Contracts;
using Bot.Contracts.Handlers;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Host;

public static class DependencyInjectionExtensions
{
    public static void RegisterEvents(this DiscordClientBuilder builder)
    {
        builder.ConfigureEventHandlers(x =>
        {
            x.HandleMessageCreated((client, args) =>
            {
                IEnumerable<IMessageCreatedEventHandler> handlers = client.ServiceProvider.GetServices<IMessageCreatedEventHandler>();
                var scopeProvider = client.ServiceProvider.GetService<IDbScopeProvider>();
                IEnumerable<Task> tasks = handlers.Select(async handler =>
                {
                    await using DbScope scope = scopeProvider!.GetDbScope();
                    await handler.Execute(client, args, scope);
                });

                _ = Task.WhenAll(tasks);
                
                return Task.CompletedTask;
            });
        });
    }
}