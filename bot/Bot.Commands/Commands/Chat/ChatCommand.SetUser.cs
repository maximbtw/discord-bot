using System.ComponentModel;
using Bot.Application.Chat.OpenAiImpersonationChat;
using Bot.Commands.Checks.Role;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Bot.Commands.Commands.Chat;

internal partial class ChatCommand
{
    [Command("as")]
    [Description("Настройки чата-имитации.")]
    [RequireGuild]
    [RoleCheck(Role.Admin, Role.Administrator)]
    public async ValueTask ExecuteSetUser(
        CommandContext context, 
        [Description("Пользователь для имитации.")] DiscordUser user)
    {
        OpenAiImpersonationChatOptions.GuildIdToImpersonationUserIdIndex[context.Guild!.Id] = user.Id;

        await context.RespondAsync($"Теперь я говорю как {user.Username}!");
    }
}