using Bot.Application.Infrastructure.Configuration;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace Bot.Application.UseCases.ServerMessages;

public class DeleteServerMessagesUseCase
{
    private readonly IDbScopeProvider _scopeProvider;
    private readonly IMessageRepository _messageRepository;
    private readonly BotConfiguration _configuration;

    public DeleteServerMessagesUseCase(
        IDbScopeProvider scopeProvider, 
        IMessageRepository messageRepository,
        BotConfiguration configuration)
    {
        _scopeProvider = scopeProvider;
        _messageRepository = messageRepository;
        _configuration = configuration;
    }

    public async ValueTask Execute(CommandContext context, long serverId, CancellationToken ct)
    {
        if (!_configuration.SaveMessagesToDb)
        {
            await context.RespondAsync("Операция не поддерживается.");
            return;
        }
        
        await context.RespondAsync("Удаление сообщений сервера начато...");
        
        await using DbScope scope = _scopeProvider.GetDbScope();
        
        await _messageRepository.DeleteServerMessages(serverId, ct);

        await scope.CommitAsync(ct);
        
        await context.EditResponseAsync(
            new DiscordWebhookBuilder()
                .WithContent($"✅ Все сообщения сервера **{context.Guild!.Name}** удалены.")
        );
    }
}