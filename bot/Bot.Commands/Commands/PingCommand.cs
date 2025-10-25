using Bot.Commands.Checks.ExecuteInDm;
using DSharpPlus.Commands;

namespace Bot.Commands.Commands;

internal class PingCommand : ICommand
{
    [Command("ping")]
    [ExecuteInDm]
    public async ValueTask Execute(CommandContext context)
    {
        await context.RespondAsync("Pong!");
    }
}