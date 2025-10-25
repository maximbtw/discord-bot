using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Bot.Application.Dataset;
using Bot.Application.Dataset.Entries;
using Bot.Contracts.Services;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.UseCases.FineTunning;

public class CreateDatasetUseCase
{
    private readonly IMessageService _messageService;
    private readonly IDbScopeProvider _scopeProvider;

    public CreateDatasetUseCase(IMessageService messageService, IDbScopeProvider scopeProvider)
    {
        _messageService = messageService;
        _scopeProvider = scopeProvider;
    }

    public async ValueTask Execute(
        CommandContext context,
        CancellationToken ct = default)
    {
        await context.RespondAsync("Начинаю готовить датасет..");

        DatasetCreator creator = new(context.User.Id, 5);
        
        await using DbScope scope = _scopeProvider.GetDbScope();

        List<MessageOrm> messages = await _messageService.GetQueryable(scope)
            .Where(x => x.GuildId == context.Guild!.Id.ToString())
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