using System.ComponentModel;
using System.Threading.Channels;
using Bot.Application.Shared;
using Bot.Commands.Checks.Role;
using Bot.Contracts.Message;
using Bot.Domain.Scope;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Commands.Commands.Data;

internal partial class DataCommand
{
    private const int MaxParallelOfDegree = 8;
    private const int MaxMessageToBulkInsert = 500;
    
    [Command("load")]
    [RoleCheck(Role.Admin)]
    [RequireGuild]
    [Description("Загружает сообщения в базу знаний.")]
    public async ValueTask ExecuteLoad(CommandContext context, params DiscordChannel[] channels)
    {
        await LoadMessages(context, channels.ToList());
    }

    [Command("loadall")]
    [RoleCheck(Role.Admin)]
    [RequireGuild]
    [Description("Загружает сообщения в базу знаний.")]
    public async ValueTask ExecuteLoad(CommandContext context)
    {
        await LoadMessages(context);
    }

    private async ValueTask LoadMessages(CommandContext context,List< DiscordChannel>? channels = null)
    {
        await context.DeferResponseAsync();

        if (channels is null)
        {
            channels = context.Guild!.Channels.Values.ToList();
        }

        var hasChannelsToLoad = await HasChannelsToLoad(context, channels);
        if (!hasChannelsToLoad)
        {
            return;
        }

        await LoadMessagesAndSaveToDb(context, channels);
    }

    private async Task<bool> HasChannelsToLoad(CommandContext context, List<DiscordChannel> channels)
    {
        HashSet<string> channelIds = channels.Select(x => x.Id.ToString()).ToHashSet();

        await using DbScope scope = _scopeProvider.GetDbScope();

        List<string> alreadyLoadedChannels = await _messageService
            .GetQueryable(scope)
            .Where(x => x.GuildId == context.Guild!.Id.ToString())
            .Where(x => channelIds.Contains(x.ChannelId))
            .Select(x => x.ChannelId)
            .Distinct()
            .ToListAsync();

        if (alreadyLoadedChannels.Count == channels.Count)
        {
            await context.RespondAsync("Все сообщения уже есть в базе знаний.");
            
            return false;
        }

        if (alreadyLoadedChannels.Count > 0)
        {
            List<string> alreadyLoadedChannelNames = channels
                .Where(x => alreadyLoadedChannels.Contains(x.Id.ToString()))
                .Select(x => x.Name)
                .ToList();

            string names = string.Join(", ", alreadyLoadedChannelNames);

            await context.RespondAsync($"Сообщения с этих каналов уже есть в базе знаний: {names}");

            channels.RemoveAll(x => alreadyLoadedChannels.Contains(x.Id.ToString()));
        }

        return true;
    }

    private async Task LoadMessagesAndSaveToDb(CommandContext context, List<DiscordChannel> channels)
    {
        var dispatcher = await ProgressMessageDispatcher.Create(context, channels.Count);

        var dbSaverChannel = Channel.CreateUnbounded<DiscordMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        Task saveToDbTask = SaveMessagesToDb(dbSaverChannel.Reader, dispatcher);

        int maxParallelOfDegree = channels.Count > MaxParallelOfDegree ? MaxParallelOfDegree : channels.Count;

        var semaphore = new SemaphoreSlim(maxParallelOfDegree);
        List<Task> tasks = channels
            .Where(x => x.Type == DiscordChannelType.Text)
            .Select(async discordChannel =>
            {
                try
                {
                    await semaphore.WaitAsync();
                    await LoadChannelMessagesAndSaveToDb(context, discordChannel, dbSaverChannel.Writer);
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
        CommandContext context, 
        DiscordChannel channel, 
        ChannelWriter<DiscordMessage> dbSaverChannelWriter)
    {
        ulong? lastMessageId = null;

        bool anyLoaded;
        do
        {
            anyLoaded = false;

            IAsyncEnumerable<DiscordMessage> messages = lastMessageId == null
                ? channel.GetMessagesAsync()
                : channel.GetMessagesBeforeAsync(before: (ulong)lastMessageId);

            await foreach (DiscordMessage message in messages)
            {
                if (!DiscordMessageHelper.MessageIsValid(message, context.Client.CurrentUser.Id))
                {
                    continue;
                }

                await dbSaverChannelWriter.WriteAsync(message);
                anyLoaded = true;
                lastMessageId = message.Id;
            }
        } while (anyLoaded);
    }

    private async Task SaveMessagesToDb(
        ChannelReader<DiscordMessage> dbSaverChannelReader,
        ProgressMessageDispatcher dispatcher)
    {
        var buffer = new List<Message>(MaxMessageToBulkInsert);

        await foreach (DiscordMessage discordMessage in dbSaverChannelReader.ReadAllAsync())
        {
            buffer.Add(DiscordContentMapper.MapDiscordMessageToMessage(discordMessage));

            if (buffer.Count >= MaxMessageToBulkInsert)
            {
                await SaveToDb();
            }
        }

        if (buffer.Count > 0)
        {
            await SaveToDb();
        }

        return;

        async Task SaveToDb()
        {
            await using DbScope scope = _scopeProvider.GetDbScope();

            await _messageService.Add(buffer, scope, CancellationToken.None);

            await scope.CommitAsync();
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

        public static async Task<ProgressMessageDispatcher> Create(CommandContext context,
            int totalChannels)
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