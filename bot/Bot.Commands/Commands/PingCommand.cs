using DSharpPlus.Commands;

namespace Bot.Commands.Commands;

internal class PingCommand : ICommand
{
    [Command("ping")]
    public async ValueTask Execute(CommandContext context)
    {
        await context.RespondAsync("Pong!");
    }
}