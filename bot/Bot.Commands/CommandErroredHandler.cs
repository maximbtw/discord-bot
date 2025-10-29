using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;

namespace Bot.Commands;

internal static class CommandErroredHandler
{
    internal static async Task HandleHandle(CommandsExtension sender, CommandErroredEventArgs args)
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