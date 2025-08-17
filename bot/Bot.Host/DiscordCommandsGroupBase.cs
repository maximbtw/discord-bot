using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using Microsoft.Extensions.Logging;

namespace Bot.Host;

internal abstract class DiscordCommandsGroupBase;

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

            await context.FollowupAsync("Something went wrong. Please try again later.");
        }
    }
}