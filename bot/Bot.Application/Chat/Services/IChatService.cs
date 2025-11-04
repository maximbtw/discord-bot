using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using Bot.Domain.Orms.Message;
using Bot.Domain.Scope;

namespace Bot.Application.Chat.Services;

public interface IChatService
{
    IQueryable<MessageOrm> GetQueryable(DbScope scope);
    
    Task AddMessage(Message message, DbScope scope, CancellationToken ct, bool saveToCache = false);
    
    Task AddMessages(List<Message> messages, DbScope scope, CancellationToken ct);
    
    Task DeleteGuildMessages(ulong guildId, List<ulong> channelIds, DbScope scope, CancellationToken ct = default);

    List<Message> GetMessagesFromCache(ulong guildId, ulong channelId);
    
    void ResetCache(ulong guildId, List<ulong> channelIds);

    Task ResetChatSettings(ulong guildId, DbScope scope, CancellationToken ct = default);
    
    Task<GuildChatSettings> GetGuildSettings(ulong guildId, DbScope scope, CancellationToken ct = default);
    
    Task UpdateOrCreateChatSettings(GuildChatSettings settings, DbScope scope, CancellationToken ct = default);
}