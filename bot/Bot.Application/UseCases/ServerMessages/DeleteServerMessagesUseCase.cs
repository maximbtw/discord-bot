using Bot.Contracts.Services;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace Bot.Application.UseCases.ServerMessages;

public class DeleteServerMessagesUseCase
{
    private readonly IDbScopeProvider _scopeProvider;
    private readonly IMessageService _messageService;

    public DeleteServerMessagesUseCase(
        IDbScopeProvider scopeProvider, 
        IMessageService messageService)
    {
        _scopeProvider = scopeProvider;
        _messageService = messageService;
    }

    public async ValueTask Execute(CommandContext context, ulong guildId, CancellationToken ct)
    {
        await context.RespondAsync("Удаление сообщений сервера начато...");
        
        await using DbScope scope = _scopeProvider.GetDbScope();
        
        await _messageService.DeleteGuildMessages(guildId, channelIds: [], scope, ct);

        await scope.CommitAsync(ct);
        
        await context.EditResponseAsync(
            new DiscordWebhookBuilder()
                .WithContent($"✅ Все сообщения сервера **{context.Guild!.Name}** удалены.")
        );
    }
}