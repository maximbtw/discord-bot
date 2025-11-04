using System.ComponentModel;
using System.Text;
using Bot.Domain.Orms.Message;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Commands.Commands.Data;

internal partial class DataCommand
{
    [Command("stats")]
    [Description("Показывает статистику которой сейчас обладает бот.")]
    [RequireGuild]
    public async ValueTask ExecuteGetStats(
        CommandContext context, 
        DiscordMember? member = null)
    {
        await context.DeferResponseAsync();

        List<UserStats> stats = await LoadStatsFromDb(context, member);
        
        var sb = new StringBuilder();

        if (member is null)
        {
            sb.AppendLine("📊 **Общая статистика пользователей:**");
        }
        else
        {
            sb.AppendLine($"📊 **Статистика для {member.Username}:**");
        }
        
        if (stats.Count > 0)
        {
            foreach (UserStats stat in stats)
            {
                sb.AppendLine($"• **{stat.UserName}** — сообщений: **{stat.TotalMessages:N0}**");
            }
        }
        else
        {
            sb.AppendLine("❌ Статистика не найдена.");
        }
        
        await context.RespondAsync(sb.ToString());
    }
    
    private async Task<List<UserStats>> LoadStatsFromDb(CommandContext context, DiscordMember? member)
    {
        await using DbScope scope = _scopeProvider.GetDbScope();
        
        IQueryable<MessageOrm> query = _chatService
            .GetQueryable(scope)
            .Where(x => x.GuildId == context.Guild!.Id.ToString());

        if (member is not null)
        {
            query = query.Where(x => x.UserId == member.Id.ToString());
        }
        
        return await query
            .GroupBy(x => x.UserId)
            .OrderByDescending(x=>x.Count())
            .Select(g => new UserStats(g.First().UserNickname, g.Count()))
            .ToListAsync();
    }
    
    private record UserStats(string UserName, int TotalMessages);
}