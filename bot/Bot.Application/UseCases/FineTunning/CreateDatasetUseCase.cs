using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Bot.Application.Dataset;
using Bot.Application.Dataset.Entries;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.UseCases.FineTunning;

public class CreateDatasetUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IDbScopeProvider _scopeProvider;

    public CreateDatasetUseCase(IMessageRepository messageRepository, IDbScopeProvider scopeProvider)
    {
        _messageRepository = messageRepository;
        _scopeProvider = scopeProvider;
    }

    public async ValueTask Execute(
        CommandContext context,
        CancellationToken ct = default)
    {
        await context.RespondAsync("Начинаю готовить датасет..");

        DatasetCreator creator = new((long)context.User.Id, 5);

        long serverId = (long)context.Guild!.Id;
        
        await using DbScope scope = _scopeProvider.GetDbScope();

        List<MessageOrm> messages = await _messageRepository
            .GetQueryable(scope)
            .Where(x => x.ServerId == serverId)
            .ToListAsync(ct);

        IEnumerable<ConversationEntry> dataset = creator.Create(messages);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) 
        };

        string json = JsonSerializer.Serialize(dataset, options);

        string filePath = Path.Combine(AppContext.BaseDirectory, "discord_chat.json");

        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8, ct);

        await context.EditResponseAsync(
            new DiscordWebhookBuilder()
                .WithContent($"Датасет сохранён.")
        );
    }
}