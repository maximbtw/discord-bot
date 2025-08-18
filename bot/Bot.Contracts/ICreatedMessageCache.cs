using Bot.Contracts.Shared;

namespace Bot.Contracts;

public interface ICreatedMessageCache
{
    void Add(ulong serverId, ulong channelId, MessageDto message);
    
    List<MessageDto> GetLastMessages(ulong serverId, ulong channelId);
}