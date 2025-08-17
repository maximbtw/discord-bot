using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Bot.Application.Dataset;
using Bot.Application.Dataset.Entries;
using Bot.Domain.Message;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.UseCases.Ai;

public class CreateDatasetUseCase
{
    private readonly IMessageRepository _messageRepository;

    public CreateDatasetUseCase(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async ValueTask Execute(
        CommandContext context,
        CancellationToken ct = default)
    {
        await context.RespondAsync("Начинаю готовить датасет..");

        DatasetCreator creator = new((long)context.User.Id, 5);

        long serverId = (long)context.Guild!.Id;

        List<MessageOrm> messages =
            await _messageRepository.GetQueryable().Where(x => x.ServerId == serverId).ToListAsync(ct);

        IEnumerable<ConversationEntry> dataset = creator.Create(messages);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) 
        };

        string json = JsonSerializer.Serialize(dataset, options);

        string filePath = Path.Combine(AppContext.BaseDirectory, "discord_chat.json");

        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8, ct);

        await context.FollowupAsync($"Датасет сохранён.");
    }
}