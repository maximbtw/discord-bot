using System.ComponentModel;
using Bot.Application.UseCases.Misc;
using DSharpPlus.Commands;
using Microsoft.Extensions.Logging;

namespace Bot.Commands;

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
}