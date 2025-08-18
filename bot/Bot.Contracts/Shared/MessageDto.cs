namespace Bot.Contracts.Shared;

public record MessageDto(
    long Id, 
    long UserId,
    string UserName, 
    bool UserIsBot, 
    long ChannelId, 
    long ServerId,
    string Content,
    DateTime Timestamp,
    long? ReplyToMessageId,
    bool HasAttachments,
    List<long> MentionedUserIds);