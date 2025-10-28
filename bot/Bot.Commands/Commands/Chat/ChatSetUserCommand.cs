using System.ComponentModel;
using Bot.Application.Chat;
using Bot.Commands.Checks.Role;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;

namespace Bot.Commands.Commands.Chat;

[Command("chat")]
internal class ChatSetUserCommand : ICommand
{
    private readonly ChatSettings _chatSettings;
    
    public ChatSetUserCommand(IConfiguration configuration)
    {
        _chatSettings = configuration.GetSection(nameof(ChatSettings)).Get<ChatSettings>()!;
    }
    
    [Command("as")]
    [Description("Настройки чата-имитации.")]
    [RequireGuild]
    [RoleCheck(Role.Admin, Role.Administrator)]
    public async ValueTask Execute(
        CommandContext context, 
        [Description("Пользователь для имитации.")] DiscordUser user)
    {
        _chatSettings.ImpersonationChatOptions.GuildIdToImpersonationUserIdIndex[context.Guild!.Id] = user.Id;

        await context.RespondAsync($"Теперь я говорю как {user.Username}!");
    }
}