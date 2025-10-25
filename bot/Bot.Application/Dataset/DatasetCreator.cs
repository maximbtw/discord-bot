using System.Text;
using System.Text.RegularExpressions;
using Bot.Application.Dataset.Entries;
using Bot.Domain.Message;

namespace Bot.Application.Dataset;

internal class DatasetCreator
{
    
    public DatasetCreator(ulong authorId, int? contextDepthInMinutes = null)
    {
    }

    public IEnumerable<ConversationEntry> Create(List<MessageOrm> messages)
    {
        throw new NotImplementedException();
        /*_userIdToUserNameIndex = messages
            .GroupBy(x => x.UserId)
            .ToDictionary(x => x.Key, x => x.First().UserNickname);

        IEnumerable<IGrouping<ulong, MessageOrm>> groupedByServer = messages.GroupBy(x => x.GuildId);
        foreach (IGrouping<ulong, MessageOrm> messagesByServer in groupedByServer)
        {
            IEnumerable<IGrouping<ulong, MessageOrm>> groupedByChannel = messagesByServer.GroupBy(x => x.ChannelId);
            foreach (IGrouping<ulong, MessageOrm> messagesByChannel in groupedByChannel)
            {
                foreach (ConversationEntry entry in CreateByChannel(messagesByChannel))
                {
                    yield return entry;
                }
            }
        }*/
    }
}