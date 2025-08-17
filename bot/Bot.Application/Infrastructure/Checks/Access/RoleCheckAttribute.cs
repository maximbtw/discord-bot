using DSharpPlus.Commands.ContextChecks;

namespace Bot.Application.Infrastructure.Checks.Access;

[AttributeUsage(AttributeTargets.Method)]
public class RoleCheckAttribute : ContextCheckAttribute
{
    public Role[] Roles { get; }

    public RoleCheckAttribute(params Role[] roles)
    {
        this.Roles = roles;
    }
}