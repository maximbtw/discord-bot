using System.Text;
using System.Text.RegularExpressions;
using Bot.Application.Dataset.Entries;
using Bot.Domain.Message;

namespace Bot.Application.Dataset;

internal class DatasetCreator
{
    private const int MaxMergedMessages = 10;
    
    private const int MaxConversations = 10;
    private const int MinConversations = 2;
    private const int MaxContextDepthInMinutes = 10_080; // 1 неделя.
    
    private readonly Regex _mentionRegex = new(@"<@!?(\d+)>", RegexOptions.Compiled);

    private readonly TimeSpan _contextWindow;
    private readonly long _authorId;
    private Dictionary<long, string> _userIdToUserNameIndex = new();

    public DatasetCreator(long authorId, int? contextDepthInMinutes = null)
    {
        _contextWindow = TimeSpan.FromMinutes(contextDepthInMinutes is null or > MaxContextDepthInMinutes
            ? MaxContextDepthInMinutes
            : (int)contextDepthInMinutes);

        _authorId = authorId;
    }

    public IEnumerable<ConversationEntry> Create(List<MessageOrm> messages)
    {
        _userIdToUserNameIndex = messages
            .GroupBy(x => x.UserId)
            .ToDictionary(x => x.Key, x => x.First().UserName);
        
        IEnumerable<IGrouping<long, MessageOrm>> groupedByServer = messages.GroupBy(x => x.ServerId);
        foreach (IGrouping<long, MessageOrm> messagesByServer in groupedByServer)
        {
            IEnumerable<IGrouping<long, MessageOrm>> groupedByChannel = messagesByServer.GroupBy(x => x.ChannelId);
            foreach (IGrouping<long, MessageOrm> messagesByChannel in groupedByChannel)
            {
                foreach (ConversationEntry entry in CreateByChannel(messagesByChannel))
                {
                    yield return entry;
                }
            }
        }
    }

    private IEnumerable<ConversationEntry> CreateByChannel(IEnumerable<MessageOrm> messages)
    {
        var entry = new ConversationEntry();
        var turnQueue = new Queue<MessageOrm>();

        var lastMessageTime = DateTime.MinValue;
        foreach (MessageOrm message in messages.OrderBy(x => x.Timestamp))
        {
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                continue;
            }

            // +1 с учетом, что добавим фразу перед обрезкой
            int currentTurnsCount = entry.Conversations.Count + 1;
            bool timeExpired = lastMessageTime != DateTime.MinValue && 
                               message.Timestamp - lastMessageTime > _contextWindow;

            bool needToSliceConversation = currentTurnsCount >= MaxConversations || timeExpired;
            if (needToSliceConversation)
            {
                bool conversationIsValid = currentTurnsCount >= MinConversations && currentTurnsCount % 2 == 0;
                if (conversationIsValid)
                {
                    entry.Conversations.Add(CreateMessageTurn(turnQueue));
                    
                    yield return entry;

                    entry = new ConversationEntry();
                    turnQueue.Clear();   
                }
            }

            if (turnQueue.TryPeek(out MessageOrm? lastMessage))
            {
                bool currentIsBot = message.UserId == _authorId;
                bool prevIsBot = lastMessage.UserId == _authorId;
                if (currentIsBot != prevIsBot)
                {
                    if (entry.Conversations.Count != 0 || !prevIsBot)
                    {
                        entry.Conversations.Add(CreateMessageTurn(turnQueue));
                    }
                    
                    turnQueue.Clear();
                }
            }

            if (turnQueue.Count > MaxMergedMessages)
            {
                turnQueue.Dequeue();
            }
            
            turnQueue.Enqueue(message);
            lastMessageTime = message.Timestamp;
        }

        if (turnQueue.Count > 0)
        {
            entry.Conversations.Add(CreateMessageTurn(turnQueue));

            yield return entry;
        }
    }

    private MessageTurn CreateMessageTurn(Queue<MessageOrm> messages)
    {
        long authorId = messages.Peek().UserId;
        return new MessageTurn
        {
            From = authorId == _authorId ? "gpt" : "human",
            Value = BuildMessage(messages)
        };
    }

    private string BuildMessage(Queue<MessageOrm> messages)
    {
        var stringBuilder = new StringBuilder();

        while (messages.TryDequeue(out MessageOrm? message))
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append('\n');
            }

            string content = message.Content!.Replace("\r\n", "\n").Trim();
            
            string normalizedContent = ReplaceMentions(content);
            
            stringBuilder.Append(normalizedContent);
        }

        return stringBuilder.ToString();
    }
    
    private string ReplaceMentions(string input)
    {
        return _mentionRegex.Replace(input, match =>
        {
            long userId = long.Parse(match.Groups[1].Value);
            return _userIdToUserNameIndex.TryGetValue(userId, out string? name) ? $"@{name}" : "@UnknownUser";
        });
    }
}