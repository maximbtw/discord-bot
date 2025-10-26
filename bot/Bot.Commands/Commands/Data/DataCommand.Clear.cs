using System.ComponentModel;
using Bot.Commands.Checks.Role;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Bot.Commands.Commands.Data;

internal partial class DataCommand
{
    [Command("clear")]
    [RoleCheck(Role.Admin)]
    [RequireGuild]
    [Description("Удаляет сообщения из базы знаний в указанных каналах.")]
    public async ValueTask ExecuteClear(
        CommandContext context,
        [Description("Каналы, из которых нужно удалить сообщения.")] params DiscordChannel[] channels)
    {
        await ClearData(context, channels);
    }
    
    // Дискорд не позволяет делать опциональные массивы, поэтому сделана отдельная команда.
    [Command("clearall")]
    [RoleCheck(Role.Admin)]
    [RequireGuild]
    [Description("Удаляет все сообщения сервера из базы знаний.")]
    public async ValueTask ExecuteClear(CommandContext context)
    {
        await ClearData(context);
    }
    
    private async Task ClearData(CommandContext context, DiscordChannel[]? channels = null)
    {
        await context.DeferResponseAsync();

        await using DbScope scope = _scopeProvider.GetDbScope();
        
        List<ulong> channelIds = channels == null ? [] : channels.Select(x => x.Id).ToList();

        await _messageService.DeleteGuildMessages(context.Guild!.Id, channelIds, scope, CancellationToken.None);

        await scope.CommitAsync();

        await context.RespondAsync($"✅ Все сообщения сервера **{context.Guild!.Name}** удалены.");
    }
}