using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Contracts.Handlers;
using Bot.Domain.Scope;
using DSharpPlus;
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

                IEnumerable<Task> tasks = handlers.Select(async handler =>
                {
                    var scopeProvider = client.ServiceProvider.GetService<IDbScopeProvider>()!;
                    await using DbScope scope = scopeProvider.GetDbScope();
                    await handler.Execute(client, args, scope);
                });

                _ = Task.WhenAll(tasks);
                
                return Task.CompletedTask;
            });
        });
    }
}