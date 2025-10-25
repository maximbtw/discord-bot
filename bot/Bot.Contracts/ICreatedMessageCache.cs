using Bot.Contracts.Shared;

namespace Bot.Contracts;

public interface ICreatedMessageCache
{
    void Add(ulong serverId, ulong channelId, Message message);
    
    List<Message> GetLastMessages(ulong serverId, ulong channelId);
}