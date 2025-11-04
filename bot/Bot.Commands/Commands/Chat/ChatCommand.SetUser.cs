using System.ComponentModel;
using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Bot.Commands.Commands.Chat;

internal partial class ChatCommand
{
    [Command("as")]
    [Description("Настройки чата-имитации.")]
    [RequireGuild]
    [RequirePermissions([],[DiscordPermission.Administrator])]
    public async ValueTask ExecuteSetUser(
        CommandContext context, 
        [Description("Пользователь для имитации.")] DiscordUser user)
    {
        DiscordGuild guild = context.Guild!;
        
        await using DbScope scope = _scopeProvider.GetDbScope();

        GuildChatSettings settings = await _chatService.GetGuildSettings(guild.Id, scope);

        if (settings.ChatType != ChatType.Impersonation)
        {
            await context.RespondAsync(
                "⚙️ Текущий режим чата не поддерживает имитацию речи.\n" +
                "Используйте команду `/chat set-settings` и измените тип на **Immersion**."
            );
            
            return;
        }

        if (settings.ImpersonationUserId == null || settings.ImpersonationUserId != user.Id)
        {
            settings.ImpersonationUserId = user.Id;

            await _chatService.UpdateOrCreateChatSettings(settings, scope);

            await scope.CommitAsync();
        }
        
        await context.RespondAsync($"Теперь я говорю как {user.Username}!");
    }
}