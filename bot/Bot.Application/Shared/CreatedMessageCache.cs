using Bot.Contracts;
using Bot.Contracts.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace Bot.Application.Shared;

internal class CreatedMessageCache : ICreatedMessageCache
{
    private const int MaxMessages = 100;
    
    private readonly IMemoryCache _cache;

    public CreatedMessageCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Add(ulong serverId, ulong channelId, ShortMessageInfo messageInfo)
    {
        var key = new Key(serverId, channelId);
        
        Queue<ShortMessageInfo> messages = _cache.GetOrCreate(key, _ => new Queue<ShortMessageInfo>())!;
        
        messages.Enqueue(messageInfo);
        
        if (messages.Count > MaxMessages)
        {
            messages.Dequeue();   
        }
    }
    
    public List<ShortMessageInfo> Get(ulong serverId, ulong channelId)
    {
        var key = new Key(serverId, channelId);
        
        return _cache.TryGetValue(key, out Queue<ShortMessageInfo>? messages)
            ? messages!.ToList()
            : Enumerable.Empty<ShortMessageInfo>().ToList();
    }

    private readonly record struct Key(ulong ServerId, ulong ChannelId);
}