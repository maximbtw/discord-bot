using System.Text;
using Bot.Application.Infrastructure.Configuration;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.UseCases.ServerMessages;

public class GetServerMessagesStatsUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IDbScopeProvider _scopeProvider;
    private readonly BotConfiguration _configuration;

    public GetServerMessagesStatsUseCase(
        IMessageRepository messageRepository,
        BotConfiguration configuration, IDbScopeProvider scopeProvider)
    {
        _messageRepository = messageRepository;
        _configuration = configuration;
        _scopeProvider = scopeProvider;
    }
    
    public async ValueTask Execute(CommandContext context, DiscordUser? user = null, CancellationToken ct = default)
    {
        if (!_configuration.UseDb)
        {
            await context.RespondAsync("Операция не поддерживается.");
            return;
        }
        
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
        await using DbScope scope = _scopeProvider.GetDbScope();
        
        IQueryable<MessageOrm> query = _messageRepository
            .GetQueryable(scope)
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