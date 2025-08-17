using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Bot.Application.UseCases.Misc;
using DSharpPlus.Commands;
using Microsoft.Extensions.Logging;

namespace Bot.Host.Commands;

internal class MiscCommands : DiscordCommandsGroupBase<MiscCommands>
{
    private readonly GetJokeUseCase _getJokeUseCase; 
    
    public MiscCommands(ILogger<MiscCommands> logger, GetJokeUseCase getJokeUseCase) : base(logger)
    {
        _getJokeUseCase = getJokeUseCase;
    }
    
    [Command("joke")]
    [Description("Случайный анекдот")]
    public async ValueTask GetJoke(CommandContext context)
    {
        await ExecuteAsync(context, () => _getJokeUseCase.Execute(context, CancellationToken.None));
    }
    
    [Command("ping")]
    public async ValueTask Ping(CommandContext context)
    {
        await context.RespondAsync("Pong!");
    }
}