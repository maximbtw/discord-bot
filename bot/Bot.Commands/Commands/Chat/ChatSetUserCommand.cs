using System.ComponentModel;
using Bot.Application.Handlers.Chat.OpenAiImpersonationChat;
using Bot.Application.Infrastructure.Configuration;
using Bot.Commands.Checks.ExecuteInDm;
using Bot.Commands.Checks.Role;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;

namespace Bot.Commands.Commands.Chat;

[Command("chat")]
internal class ChatSetUserCommand : ICommand
{
    private readonly OpenAiImpersonationChatOptions _chatOptions;
    
    public ChatSetUserCommand(IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(OpenAiSettings)).Get<OpenAiSettings>()!;

        _chatOptions = settings.ChatOptions.ImpersonationChatOptions;
    }
    
    [Command("as")]
    [Description("Настройки чата-имитации.")]
    [ExecuteInDm]
    [RoleCheck(Role.Admin, Role.Administrator)]
    public async ValueTask Execute(
        CommandContext context, 
        [Description("Пользователь для имитации.")] DiscordUser user)
    {
        _chatOptions.GuildIdToImpersonationUserIdIndex[context.Guild!.Id] = user.Id;

        await context.RespondAsync($"Теперь я говорю как {user.Username}!");
    }
}