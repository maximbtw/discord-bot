using System.Text;
using Bot.Domain.Message;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.UseCases.ServerMessages;

public class GetServerMessagesStatsUseCase
{
    private readonly IMessageRepository _messageRepository;

    public GetServerMessagesStatsUseCase(
        IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }
    
    public async ValueTask Execute(CommandContext context, DiscordUser? user = null, CancellationToken ct = default)
    {
        await context.RespondAsync("Ищу статистику...");
        
        List<UserStats> stats = await GetStats((long)context.Guild!.Id, (long?)user?.Id, ct);
        
        var sb = new StringBuilder();
        if (user == null)
        {
            sb.AppendLine("📊 Общая статистика пользователей:");
        }
        else
        {
            sb.AppendLine($"📊 Статистика для пользователя **{user.Username}**:");
        }

        if (stats.Count > 0)
        {
            foreach (UserStats stat in stats)
            {
                sb.AppendLine($"• **{stat.UserName}** — сообщений: **{stat.TotalMessages}**");
            }
        }
        else
        {
            sb.AppendLine("❌ Статистика не найдена.");
        }
        
        await context.EditResponseAsync(new DiscordWebhookBuilder().WithContent(sb.ToString()));
    }

    private async Task<List<UserStats>> GetStats(long serverId, long? userId, CancellationToken ct)
    {
        IQueryable<MessageOrm> query = _messageRepository
            .GetQueryable()
            .Where(x => x.ServerId == serverId && !x.UserIsBot);

        if (userId != null)
        {
            query = query.Where(x => x.UserId == userId);
        }
        
        return await query
            .GroupBy(x => x.UserId)
            .OrderByDescending(x=>x.Count())
            .Select(g => new UserStats(g.First().UserName, g.Count()))
            .ToListAsync(ct);
    }
    
    private record UserStats(string UserName, int TotalMessages);
}