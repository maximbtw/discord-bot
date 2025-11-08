using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bot.Commands;

internal class CommandErroredHandler
{
    private readonly ILogger<CommandErroredHandler> _logger;

    public CommandErroredHandler(ILogger<CommandErroredHandler> logger)
    {
        _logger = logger;
    }
    
    internal async Task HandleHandle(CommandsExtension sender, CommandErroredEventArgs args)
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
        
        _logger.LogError(args.Exception,
            "Unexpected error occurred while executing command '{Command}' by user '{User}'",
            args.Context.Command.Name,
            args.Context.User.Username);
        
        await args.Context.RespondAsync("У меня не получилось(");
    }
}