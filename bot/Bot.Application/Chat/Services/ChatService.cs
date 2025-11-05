using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using Bot.Domain.Orms.Message;
using Bot.Domain.Scope;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Bot.Application.Chat.Services;

internal class ChatService : IChatService
{
    private const int MaxMessagesInCache = 100;
    
    private readonly IMessageRepository _messageRepository;
    private readonly IChatSettingsRepository _chatSettingsRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ChatSettings _chatSettings;

    public ChatService(
        IMessageRepository messageRepository, 
        IChatSettingsRepository chatSettingsRepository, 
        IMemoryCache memoryCache, 
        IConfiguration configuration)
    {
        _messageRepository = messageRepository;
        _chatSettingsRepository = chatSettingsRepository;
        _memoryCache = memoryCache;
        
        _chatSettings = configuration.GetSection(nameof(ChatSettings)).Get<ChatSettings>()!;
    }

    public IQueryable<MessageOrm> GetQueryable(DbScope scope)
    {
        return _messageRepository.GetQueryable(scope);
    }

    public async Task AddMessage(Message message, DbScope scope, CancellationToken ct = default, bool saveToCache = false)
    {
        MessageOrm orm = MapToOrm(message);

        await _messageRepository.Insert(orm, scope, ct);

        if (saveToCache)
        {
            SaveToCache(message);
        }
    }

    public async Task AddMessages(List<Message> messages, DbScope scope, CancellationToken ct = default)
    {
        List<MessageOrm> orms = messages.ConvertAll(MapToOrm);

        if (scope.SupportTransaction)
        {
            await _messageRepository.BulkInsert(orms, scope, ct);
        }
        else
        {
            foreach (MessageOrm orm in orms)
            {
                await _messageRepository.Insert(orm, scope, ct);
            }
        }
    }

    public async Task DeleteGuildMessages(
        ulong guildId,
        List<ulong> channelIds,
        DbScope scope,
        CancellationToken ct = default)
    {
        IQueryable<MessageOrm> updateQueryable = _messageRepository.GetUpdateQueryable(scope);

        IQueryable<MessageOrm> query = updateQueryable.Where(m => m.GuildId == guildId.ToString());
        if (channelIds.Any())
        {
            query = query.Where(m => channelIds.Any(y => y.ToString() == m.ChannelId));
        }

        await query.ExecuteDeleteAsync(ct);
    }
    
    public List<Message> GetMessagesFromCache(ulong guildId, ulong channelId)
    {
        var key = new CacheKey(guildId, channelId);
        
        return _memoryCache.TryGetValue(key, out Queue<Message>? messages)
            ? messages!.ToList()
            : Enumerable.Empty<Message>().ToList();
    }

    public void ResetCache(ulong guildId, List<ulong> channelIds)
    {
        foreach (ulong channelId in channelIds)
        {
            var key = new CacheKey(guildId, channelId);
            
            _memoryCache.Remove(key);
        }
    }

    public async Task ResetChatSettings(ulong guildId, DbScope scope, CancellationToken ct = default)
    {
        IQueryable<ChatSettingsOrm> query = _chatSettingsRepository
            .GetUpdateQueryable(scope)
            .Where(x => x.GuildId == guildId.ToString());

        await query.ExecuteDeleteAsync(ct);
    }

    public async Task<GuildChatSettings> GetGuildSettings(ulong guildId, DbScope scope, CancellationToken ct = default)
    {
        ChatSettingsOrm? guildSettings = await _chatSettingsRepository
            .GetQueryable(scope)
            .FirstOrDefaultAsync(x => x.GuildId == guildId.ToString(), ct);

        return new GuildChatSettings
        {
            GuildId = guildId,
            ChatType = guildSettings?.ChatType ?? _chatSettings.DefaultChatType,
            ChatHistoryLimit = guildSettings?.ChatHistoryLimit ?? _chatSettings.DefaultChatHistoryLimit,
            ResponseChance = guildSettings?.ResponseChance ?? _chatSettings.DefaultResponseChance,
            ImpersonationUserId = guildSettings?.ImpersonationUserId == null
                ? null
                : ulong.Parse(guildSettings.ImpersonationUserId)
        };
    }

    public async Task UpdateOrCreateChatSettings(GuildChatSettings settings,  DbScope scope, CancellationToken ct = default)
    {
        ChatSettingsOrm? guildSettings = await _chatSettingsRepository
            .GetUpdateQueryable(scope)
            .FirstOrDefaultAsync(x => x.GuildId == settings.GuildId.ToString(), ct);

        if (guildSettings is null)
        {
            guildSettings = new ChatSettingsOrm
            {
                GuildId = settings.GuildId.ToString(),
                ChatType = settings.ChatType,
                ChatHistoryLimit = settings.ChatHistoryLimit,
                ResponseChance = settings.ResponseChance,
                ImpersonationUserId = settings.ImpersonationUserId?.ToString() ?? null
            };
            
            await _chatSettingsRepository.Insert(guildSettings, scope, ct);
            
            return;
        }
        
        guildSettings.ChatType = settings.ChatType;
        guildSettings.ResponseChance = settings.ResponseChance;
        guildSettings.ChatHistoryLimit = settings.ChatHistoryLimit;
        guildSettings.ReplaceMentions = settings.ReplaceMentions;
        guildSettings.ImpersonationUserId = settings.ImpersonationUserId?.ToString() ?? null;
    }

    private void SaveToCache(Message message)
    {
        var key = new CacheKey(message.GuildId, message.ChannelId);
        
        Queue<Message> messages = _memoryCache.GetOrCreate(key, _ => new Queue<Message>())!;
        
        messages.Enqueue(message);
        
        if (messages.Count > MaxMessagesInCache)
        {
            messages.Dequeue();   
        }
    }

    private MessageOrm MapToOrm(Message message) => new()
    {
        Id = message.Id.ToString(),
        UserId = message.UserId.ToString(),
        UserNickname = message.UserNickname,
        UserIsBot = message.UserIsBot,
        ChannelId = message.ChannelId.ToString(),
        GuildId = message.GuildId.ToString(),
        Content = message.Content,
        Timestamp = message.Timestamp,
        ReplyToMessageId = message.ReplyToMessageId.ToString(),
        HasAttachments = message.HasAttachments,
        MentionedUserIds = message.MentionedUserIds.ConvertAll(x => x.ToString())
    };
    
    private readonly record struct CacheKey(ulong GuildId, ulong ChannelId);
}