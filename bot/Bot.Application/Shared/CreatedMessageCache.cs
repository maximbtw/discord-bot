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

    public void Add(ulong serverId, ulong channelId, MessageDto message)
    {
        var key = new Key(serverId, channelId);
        
        Queue<MessageDto> messages = _cache.GetOrCreate(key, _ => new Queue<MessageDto>())!;
        
        messages.Enqueue(message);
        
        if (messages.Count > MaxMessages)
        {
            messages.Dequeue();   
        }
    }
    
    public List<MessageDto> GetLastMessages(ulong serverId, ulong channelId)
    {
        var key = new Key(serverId, channelId);
        
        return _cache.TryGetValue(key, out Queue<MessageDto>? messages)
            ? messages!.ToList()
            : Enumerable.Empty<MessageDto>().ToList();
    }

    private readonly record struct Key(ulong ServerId, ulong ChannelId);
}