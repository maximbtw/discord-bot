using System.Reflection;
using Bot.Commands.Checks.Role;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
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
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false } &&
                    (typeof(DiscordCommandsGroupBase).IsAssignableFrom(t) || t.GetInterface(nameof(ICommand)) != null));

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

            extension.CommandErrored += HandleError;
        }, commandsConfiguration);
    }

    private static async Task HandleError(CommandsExtension sender, CommandErroredEventArgs args)
    {
        if (args.Exception is CommandNotFoundException)
        {
            await args.Context.RespondAsync("Такой команды нет");
            return;
        }
        if (args.Exception is ChecksFailedException)
        {
            await args.Context.RespondAsync("У тебя нет прав!");
            return;
        }
        
        await args.Context.RespondAsync("У меня не получилось(");
    }
}