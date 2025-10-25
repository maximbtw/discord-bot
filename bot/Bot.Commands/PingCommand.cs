using Bot.Commands.Checks.ExecuteInDm;
using DSharpPlus.Commands;
using Microsoft.Extensions.Logging;

namespace Bot.Commands;

internal class PingCommand : DiscordCommandsGroupBase<PingCommand>
{
    public PingCommand(ILogger<PingCommand> logger) : base(logger)
    {
    }

    [Command("ping")]
    [ExecuteInDm]
    public async ValueTask Execute(CommandContext context)
    {
        await context.RespondAsync("Pong!");
    }
}