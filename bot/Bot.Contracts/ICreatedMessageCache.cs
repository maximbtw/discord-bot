using Bot.Contracts.Shared;

namespace Bot.Contracts;

public interface ICreatedMessageCache
{
    void Add(ulong serverId, ulong channelId, ShortMessageInfo messageInfo);
    
    List<ShortMessageInfo> Get(ulong serverId, ulong channelId);
}