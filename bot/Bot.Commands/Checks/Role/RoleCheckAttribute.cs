using DSharpPlus.Commands.ContextChecks;

namespace Bot.Commands.Checks.Role;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class RoleCheckAttribute : ContextCheckAttribute
{
    public Role[] Roles { get; }

    public RoleCheckAttribute(params Role[] roles)
    {
        this.Roles = roles;
    }
}