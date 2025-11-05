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
    [Command("settings")]
    [Description("Показывает текущие настройки чата.")]
    [RequireGuild]
    [RequirePermissions([], [DiscordPermission.Administrator])]
    public async ValueTask ShowChatSettings(CommandContext context)
    {
        DiscordGuild guild = context.Guild!;
    
        await using DbScope scope = _scopeProvider.GetDbScope();
        GuildChatSettings settings = await _chatService.GetGuildSettings(guild.Id, scope);

        string msg =
            $"**Текущие настройки чата:**\n" +
            $"- Тип чата: `{settings.ChatType}`\n" +
            $"- Шанс ответа: `{settings.ResponseChance.ToString()}`%\n" +
            $"- Лимит истории: `{settings.ChatHistoryLimit.ToString()}`\n" +
            $"- Заменять упоминания: `{(settings.ReplaceMentions ? "Да" : "Нет")}`";

        await context.RespondAsync(msg);
    }
    
    [Command("set-settings")]
    [Description("Изменяет настройки чата.")]
    [RequireGuild]
    [RequirePermissions([], [DiscordPermission.Administrator])]
    public async ValueTask SetChatSettings(
        CommandContext context,
        [Description("Тип чата (Default, Roleplay, etc.)")] ChatType? chatType = null,
        [Description("Шанс ответа (0-100)")]                        int? responseChance = null,
        [Description("Лимит истории сообщений (1-40)")]     int? chatHistoryLimit = null,
        [Description("Заменять упоминания (true/false)")]   bool? replaceMentions = null)
    {
        bool valid = await Validate();
        if (!valid)
        {
            return;
        }
        
        DiscordGuild guild = context.Guild!;

        await using DbScope scope = _scopeProvider.GetDbScope();
        GuildChatSettings settings = await _chatService.GetGuildSettings(guild.Id, scope);

        settings.ChatType = chatType ?? settings.ChatType;
        settings.ResponseChance = responseChance ?? settings.ResponseChance;
        settings.ChatHistoryLimit = chatHistoryLimit ?? settings.ChatHistoryLimit;
        settings.ReplaceMentions = replaceMentions ?? settings.ReplaceMentions;

        await _chatService.UpdateOrCreateChatSettings(settings, scope);
        
        await scope.CommitAsync();

        await context.RespondAsync("Настройки успешно обновлены!");
        return;

        async Task<bool> Validate()
        {
            if (responseChance is < 0 or > 100)
            {
                await context.RespondAsync("Шанс ответа должен быть от 0 до 100");
                return false;
            }

            if (chatHistoryLimit is < 1 or > 40)
            {
                await context.RespondAsync("Лимит истории должен быть от 0 до 40");
                return false;
            }
            
            return true;
        }
    }
}