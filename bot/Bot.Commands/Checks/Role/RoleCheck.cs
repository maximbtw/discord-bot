using Bot.Application.Infrastructure.Configuration;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;

namespace Bot.Commands.Checks.Role;

internal class RoleCheck : IContextCheck<RoleCheckAttribute>
{
    private readonly BotConfiguration _configuration;
    
    public RoleCheck(IConfiguration configuration)
    {
        _configuration = configuration.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>()!;
    }
    
    public ValueTask<string?> ExecuteCheckAsync(RoleCheckAttribute attribute, CommandContext context)
    {
        foreach (Role role in attribute.Roles)
        {
            if (role == Role.Admin)
            {
                if (IsAdmin(context))
                {
                    return ValueTask.FromResult<string?>(null);
                }
            }
            else if (role == Role.Administrator)
            {
                if (IsAdministrator(context))
                {
                    return ValueTask.FromResult<string?>(null);
                }

                return ValueTask.FromResult<string?>("You don't have permission to use this command.");
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        
        return ValueTask.FromResult<string?>(null);
    }

    private bool IsAdmin(CommandContext context)
    {
        return context.User.Username == _configuration.AdminUsername;
    }
    
    // TODO: рекомендуют не делать запросы на апи в check, гужено использовать другой аттбриут RequiredPermission
    private bool IsAdministrator(CommandContext context)
    {
        DiscordMember member = context.Guild?.GetMemberAsync(context.User.Id).Result!;

        return member.Roles.Any(r => r.Name.Equals("Administrator", StringComparison.OrdinalIgnoreCase));
    }
}