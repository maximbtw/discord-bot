using Bot.Domain.Scope;

namespace Bot.Domain.Orms.ChatSettings;

public interface IChatSettingsRepository
{
    Task Insert(ChatSettingsOrm message, DbScope scope, CancellationToken ct);

    IQueryable<ChatSettingsOrm> GetUpdateQueryable(DbScope scope);

    IQueryable<ChatSettingsOrm> GetQueryable(DbScope scope);
}