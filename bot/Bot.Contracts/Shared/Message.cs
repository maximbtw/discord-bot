namespace Bot.Contracts.Shared;

public record Message(
    ulong Id, 
    ulong UserId,
    string UserNickname, 
    bool UserIsBot, 
    ulong ChannelId, 
    ulong GuildId,
    string Content,
    DateTime Timestamp,
    ulong? ReplyToMessageId,
    bool HasAttachments,
    List<ulong> MentionedUserIds);