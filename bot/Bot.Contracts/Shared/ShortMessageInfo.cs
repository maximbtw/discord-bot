namespace Bot.Contracts.Shared;

public record ShortMessageInfo(ulong Id, string Content, ulong UserId, string UserName, bool UserIsBot, DateTime CreatedAt);