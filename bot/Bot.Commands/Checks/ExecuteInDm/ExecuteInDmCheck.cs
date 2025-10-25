using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

namespace Bot.Commands.Checks.ExecuteInDm;

internal class ExecuteInDmCheck : IContextCheck<ExecuteInDmAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(ExecuteInDmAttribute attribute, CommandContext context)
    {
        bool isDirectMessage = context.Channel.IsPrivate;

        if (attribute.Allowed || !isDirectMessage)
        {
            return attribute.OnlyInDm && !isDirectMessage
                ? ValueTask.FromResult<string?>("Эта команда доступна только в личных сообщениях.")
                : ValueTask.FromResult<string?>(null);
        }
        
        return ValueTask.FromResult<string?>("Эта команда недоступна в личных сообщениях.");
    }
}