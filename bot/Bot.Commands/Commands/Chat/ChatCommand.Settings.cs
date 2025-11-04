using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [Description("Тип чата (Default, Roleplay, etc.)")]                 ChatType? chatType = null,
        [Description("Шанс ответа")]                        [Range(0, 100)] int? responseChance = null,
        [Description("Лимит истории сообщений (1-40)")]     [Range(0, 40)]  int? chatHistoryLimit = null,
        [Description("Заменять упоминания (true/false)")]                   bool? replaceMentions = null)
    {
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
    }
}