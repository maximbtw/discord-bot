using System.Threading.Channels;
using Bot.Application.Shared;
using Bot.Domain.Message;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.UseCases.ServerMessages;

public class LoadServerMessagesUseCase
{
    private readonly DiscordClient _client;
    private readonly IMessageRepository _messageRepository;
    private readonly IDbScopeProvider _scopeProvider;

    public LoadServerMessagesUseCase(
        IMessageRepository messageRepository,
        DiscordClient client,
        IDbScopeProvider scopeProvider)
    {
        _messageRepository = messageRepository;
        _client = client;
        _scopeProvider = scopeProvider;
    }

    public async ValueTask Execute(CommandContext context, List<DiscordChannel>? channels = null, CancellationToken ct = default)
    {
        DiscordGuild server = context.Guild!;

        if (!_client.Guilds.TryGetValue(server.Id, out DiscordGuild? guild))
        {
            await context.RespondAsync("Ошибка: сервер не найден.");
            return;
        }

        bool messagesExist = false;
        await using (DbScope scope = _scopeProvider.GetDbScope())
        {
            messagesExist = await _messageRepository
                .GetQueryable(scope)
                .AnyAsync(x => x.ServerId == (long)server.Id, ct);   
        }

        if (messagesExist)
        {
            await context.RespondAsync(
                "Сообщения сервера уже существуют, сначала удалите их командой `message-clear`.");
            return;
        }

        await LoadMessagesAndSaveToDb(context, guild, channels, ct);
    }

    private async Task LoadMessagesAndSaveToDb(
        CommandContext context,
        DiscordGuild guild,
        List<DiscordChannel>? channels = null,
        CancellationToken ct = default)
    {
        int channelsCount = channels?.Count ?? guild.Channels.Count(x => x.Value.Type == DiscordChannelType.Text);
        
        var dispatcher = await ProgressMessageDispatcher.Create(context, channelsCount);

        var dbSaverChannel = Channel.CreateUnbounded<DiscordMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        Task saveToDbTask = SaveMessagesToDb(dbSaverChannel.Reader, dispatcher, ct);

        var semaphore = new SemaphoreSlim(8);
        List<Task> tasks = (channels ?? guild.Channels.Values)
            .Where(x => x.Type == DiscordChannelType.Text)
            .Select(async discordChannel =>
            {
                try
                {
                    await semaphore.WaitAsync(ct);
                    await LoadChannelMessagesAndSaveToDb(discordChannel, dbSaverChannel.Writer, ct);
                }
                finally
                {
                    await dispatcher.ChannelComplete();

                    semaphore.Release();
                }
            }).ToList();

        await Task.WhenAll(tasks);

        dbSaverChannel.Writer.Complete();

        await saveToDbTask;

        await dispatcher.Complete();
    }

    private async Task LoadChannelMessagesAndSaveToDb(
        DiscordChannel channel,
        ChannelWriter<DiscordMessage> dbSaverChannelWriter,
        CancellationToken ct)
    {
        ulong? lastMessageId = null;

        bool anyLoaded;
        do
        {
            anyLoaded = false;

            IAsyncEnumerable<DiscordMessage> messages = lastMessageId == null
                ? channel.GetMessagesAsync(cancellationToken: ct)
                : channel.GetMessagesBeforeAsync(before: (ulong)lastMessageId, cancellationToken: ct);

            await foreach (DiscordMessage message in messages.WithCancellation(ct))
            {
                if (!DiscordMessageHelper.MessageIsValid(message, _client.CurrentUser.Id))
                {
                    continue;
                }

                await dbSaverChannelWriter.WriteAsync(message, ct);
                anyLoaded = true;
                lastMessageId = message.Id;
            }
        } while (anyLoaded);
    }

    private async Task SaveMessagesToDb(
        ChannelReader<DiscordMessage> dbSaverChannelReader,
        ProgressMessageDispatcher dispatcher,
        CancellationToken ct)
    {
        const int maxMessageToBulkInsert = 500;
        var buffer = new List<MessageOrm>(maxMessageToBulkInsert);

        await foreach (DiscordMessage discordMessage in dbSaverChannelReader.ReadAllAsync(ct))
        {
            buffer.Add(DiscordContentMapper.MapDiscordMessage(discordMessage));

            if (buffer.Count >= maxMessageToBulkInsert)
            {
                await SaveToDb();
            }
        }

        if (buffer.Count > 0)
        {
            await SaveToDb();
        }

        async Task SaveToDb()
        {
            await using DbScope scope = _scopeProvider.GetDbScope();
            await _messageRepository.BulkInsert(buffer, scope, ct);
            await scope.CommitAsync(ct);
            await dispatcher.AddSavedMessages(buffer.Count);

            buffer.Clear();
        }
    }

    private class ProgressMessageDispatcher
    {
        private readonly CommandContext _context;

        private readonly int _totalChannels;
        private int _processedChannels;
        private int _processedMessages;

        private ProgressMessageDispatcher(CommandContext context, int totalChannels)
        {
            _context = context;
            _totalChannels = totalChannels;
        }

        public static async Task<ProgressMessageDispatcher> Create(CommandContext context, int totalChannels)
        {
            var dispatcher = new ProgressMessageDispatcher(context, totalChannels);

            await context.RespondAsync(new DiscordWebhookBuilder()
                .WithContent(dispatcher.GetProgressString(done: false)));

            return dispatcher;
        }

        public async Task ChannelComplete()
        {
            Interlocked.Increment(ref _processedChannels);

            await _context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent(GetProgressString(done: false)));
        }

        public async Task AddSavedMessages(int messagesCount)
        {
            Interlocked.Add(ref _processedMessages, messagesCount);

            await _context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent(GetProgressString(done: false)));
        }

        public async Task Complete()
        {
            await _context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent(GetProgressString(done: true)));
        }

        private string GetProgressString(bool done)
        {
            int percent = _totalChannels == 0
                ? 100
                : (int)((double)_processedChannels / _totalChannels * 100);

            string bar = BuildProgressBar(percent);

            if (!done)
            {
                return $"📡 Загрузка данных...\n" +
                       $"{bar} {percent}%\n" +
                       $"Каналов: {_processedChannels}/{_totalChannels}\n" +
                       $"Сообщений: {_processedMessages:N0}";
            }

            return $"✅ Загрузка завершена!\n" +
                   $"Каналов обработано: **{_processedChannels}/{_totalChannels}**\n" +
                   $"Сообщений сохранено: **{_processedMessages:N0}**";
        }

        private static string BuildProgressBar(int percent, int size = 20)
        {
            int filled = percent * size / 100;
            int empty = size - filled;
            return $"[{new string('█', filled)}{new string('░', empty)}]";
        }
    }
}