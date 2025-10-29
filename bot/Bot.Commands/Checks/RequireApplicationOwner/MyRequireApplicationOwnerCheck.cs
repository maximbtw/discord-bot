using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

namespace Bot.Commands.Checks.RequireApplicationOwner;

internal sealed class MyRequireApplicationOwnerCheck : IContextCheck<MyRequireApplicationOwnerAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(MyRequireApplicationOwnerAttribute attribute, CommandContext context) =>
        ValueTask.FromResult(context.Client.CurrentApplication.Owners?.Contains(context.User) == true || context.User.Id == context.Client.CurrentUser.Id
            ? null
            : "This command must be executed by an owner of the application."
        );
}
