using System.ComponentModel;
using Bot.Commands.Checks.RequireApplicationOwner;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Bot.Commands.Commands.Data;

internal partial class DataCommand
{
    [Command("clear")]
    [MyRequireApplicationOwner]
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
    [MyRequireApplicationOwner]
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

        if (channelIds.Count == 0)
        {
            await context.RespondAsync($"✅ Все сообщения сервера **{context.Guild!.Name}** удалены.");
        }
        else
        {
            await context.RespondAsync($"✅ Сообщения удалены из {channelIds.Count} канал(ов) сервера **{context.Guild!.Name}**.");
        }
    }
}