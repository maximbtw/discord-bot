using System.Reflection;
using Bot.Commands.Checks.RequireApplicationOwner;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;

namespace Bot.Commands;

public static class DependencyInjectionExtensions
{
    public static void RegisterCommands(this DiscordClientBuilder builder, string prefix)
    {
        var commandsConfiguration = new CommandsConfiguration
        {
            UseDefaultCommandErrorHandler = false
        };

        builder.UseCommands((_, extension) =>
        {
            IEnumerable<Type> commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterface(nameof(ICommand)) != null);

            foreach (Type type in commandTypes)
            {
                extension.AddCommands(type);
            }

            TextCommandProcessor textCommandProcessor = new(new TextCommandConfiguration
            {
                PrefixResolver = new DefaultPrefixResolver(allowMention: true, prefix).ResolvePrefixAsync,
            });

            extension.AddProcessor(textCommandProcessor);

            extension.AddCheck<MyRequireApplicationOwnerCheck>();

            extension.CommandErrored += CommandErroredHandler.HandleHandle;
        }, commandsConfiguration);
    }
}