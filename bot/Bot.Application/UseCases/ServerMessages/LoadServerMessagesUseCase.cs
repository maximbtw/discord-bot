using System.Threading.Channels;
using Bot.Application.Shared;
using Bot.Domain.Message;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.UseCases.ServerMessages;

public class LoadServerMessagesUseCase
{
    private readonly DiscordClient _client;
    private readonly IMessageRepository _messageRepository;

    public LoadServerMessagesUseCase(
        IMessageRepository messageRepository,
        DiscordClient client)
    {
        _messageRepository = messageRepository;
        _client = client;
    }

    public async ValueTask Execute(CommandContext context, int maxDegreeOfParallel, CancellationToken ct)
    {
        DiscordGuild server = context.Guild!;

        if (!_client.Guilds.TryGetValue(server.Id, out DiscordGuild? guild))
        {
            await context.RespondAsync("Ошибка: сервер не найден.");
            return;
        }

        bool messagesExist = await _messageRepository
            .GetQueryable()
            .AnyAsync(x => x.ServerId == (long)server.Id, ct);

        if (messagesExist)
        {
            await context.RespondAsync(
                "Сообщения сервера уже существуют, сначала удалите их командой `message-clear`.");
            return;
        }

        int channelsCount = guild.Channels.Count(x => x.Value.Type == DiscordChannelType.Text);

        await context.RespondAsync($"Загрузка сообщений для сервера **{server.Name}**. Всего каналов: {channelsCount}");

        await LoadMessagesAndSaveToDb(context, guild, maxDegreeOfParallel, channelsCount, ct);

        await context.FollowupAsync($"✅ Загрузка сообщений сервера **{guild.Name}** завершена.");
    }

    private async Task LoadMessagesAndSaveToDb(
        CommandContext context,
        DiscordGuild guild,
        int maxDegreeOfParallel,
        int channelsCount,
        CancellationToken ct)
    {
        var dispatcher = await ProgressMessageDispatcher.Create(context, channelsCount);

        var dbSaverChannel = Channel.CreateUnbounded<DiscordMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        Task saveToDbTask = SaveMessagesToDb(dbSaverChannel.Reader, dispatcher, ct);

        var semaphore = new SemaphoreSlim(maxDegreeOfParallel);
        List<Task> tasks = guild.Channels.Values
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
            dispatcher.IncrementTotalMessages();

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
            await _messageRepository.BulkInsert(buffer, ct);
            await dispatcher.MessagesComplete(buffer.Count);

            buffer.Clear();
        }
    }

    private class ProgressMessageDispatcher
    {
        private readonly DiscordMessage _channelProgressMessage;
        private readonly DiscordMessage _messagesProgressMessage;

        private readonly int _totalChannels;
        private int _processedChannels;
        private int _totalMessages;
        private int _processedMessages;

        public static async Task<ProgressMessageDispatcher> Create(CommandContext context, int totalChannels)
        {
            DiscordMessage channelProgressMessage = await context.Channel
                .SendMessageAsync(GetLoadProgressString(processedChannels: 0, totalChannels));

            DiscordMessage messagesProgressMessage = await context.Channel
                .SendMessageAsync(GetSaveProgressString(0, 0));

            return new ProgressMessageDispatcher(totalChannels, channelProgressMessage, messagesProgressMessage);
        }

        private ProgressMessageDispatcher(
            int totalChannels,
            DiscordMessage channelProgressMessage,
            DiscordMessage messagesProgressMessage)
        {
            _totalChannels = totalChannels;
            _channelProgressMessage = channelProgressMessage;
            _messagesProgressMessage = messagesProgressMessage;
        }

        public async Task ChannelComplete()
        {
            Interlocked.Increment(ref _processedChannels);

            await _channelProgressMessage.ModifyAsync(GetLoadProgressString(_processedChannels, _totalChannels));
        }

        public void IncrementTotalMessages()
        {
            Interlocked.Increment(ref _totalMessages);
        }

        public async Task MessagesComplete(int messagesCount)
        {
            Interlocked.Add(ref _processedMessages, messagesCount);

            await _messagesProgressMessage.ModifyAsync(GetSaveProgressString(_processedMessages, _totalMessages));
        }

        private static string GetLoadProgressString(int processedChannels, int totalChannels) =>
            $"Загружено каналов: {processedChannels}/{totalChannels} ({processedChannels * 100 / totalChannels}%)";

        private static string GetSaveProgressString(int processedMessages, int totalMessages)
        {
            int percent = totalMessages == 0 ? 0 : processedMessages * 100 / totalMessages;
            
            return $"Сохранено сообщений: {processedMessages}/{totalMessages} ({percent}%)";
        }
    }
}