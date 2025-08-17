using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

namespace Bot.Application.Infrastructure.Checks.Access;

public class RoleCheck : IContextCheck<RoleCheckAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(RoleCheckAttribute attribute, CommandContext context)
    {
        if (attribute.Roles.Contains(Role.SuperUser))
        {
            if (context.User.Username != "maximbtw")
            {
                return ValueTask.FromResult<string?>($"You don't have permission to use this command.");
            }
        }
        
        return ValueTask.FromResult<string?>(null);
    }
}