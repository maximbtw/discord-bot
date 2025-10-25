using DSharpPlus.Commands;
using Microsoft.Extensions.Logging;

namespace Bot.Commands;

[Obsolete]
internal abstract class DiscordCommandsGroupBase;

[Obsolete]
internal abstract class DiscordCommandsGroupBase<TGroup> : DiscordCommandsGroupBase
{
    protected readonly ILogger<TGroup> Logger;
    
    protected DiscordCommandsGroupBase(ILogger<TGroup> logger)
    {
        this.Logger = logger;
    }
    
    internal async ValueTask ExecuteAsync(CommandContext context, Func<ValueTask> func)
    {
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Unexpected error while executing command.");
        }
    }
}