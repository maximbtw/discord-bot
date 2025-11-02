using System.ClientModel;
using System.Collections.Frozen;
using Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;
using Bot.Application.Jobs.SteamNewReleasesLoader.Service;
using Bot.Domain.Orms.SteamNewReleasesSettings;
using Bot.Domain.Scope;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Quartz;

namespace Bot.Application.Jobs.SteamNewReleasesLoader;

[DisallowConcurrentExecution]
internal class SteamNewReleasesLoaderJob : IJob
{
    private readonly ISteamNewReleasesService _service;
    private readonly DiscordClient _discordClient ;
    private readonly SteamNewReleasesLoaderSettings _settings;
    private readonly IDbScopeProvider _dbScopeProvider;
    private readonly ChatClient _client;
    private readonly ILogger<SteamNewReleasesLoaderJob> _logger;
    private readonly FrozenSet<string> _loadCategories;

    public SteamNewReleasesLoaderJob(
        ISteamNewReleasesService service, 
        DiscordClient discordClient,
        IConfiguration configuration, 
        IDbScopeProvider dbScopeProvider, 
        ILogger<SteamNewReleasesLoaderJob> logger, 
        ChatClient client)
    {
        _service = service; 
        _discordClient = discordClient;
        _dbScopeProvider = dbScopeProvider;
        _logger = logger;
        _client = client;
        _settings =  configuration.GetSection(nameof(SteamNewReleasesLoaderSettings)).Get<SteamNewReleasesLoaderSettings>()!;
        
        _loadCategories = _settings.LoadCategories.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting SteamNewReleasesLoaderJob.");
        try
        {
            await ExecuteCore();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error executing SteamNewReleasesLoaderJob");
        }
        
        _logger.LogInformation("Finished SteamNewReleasesLoaderJob.");
    }

    private async Task ExecuteCore()
    {
        List<SteamNewReleasesSettingsOrm> settings = await GetGuildsSettings();
        if (!settings.Any())
        {
            return;
        }

        var skipGuildIds = new HashSet<string>();
        string? lastAppId = null;
        await foreach (string appId in _service.GetLastAppIds(_settings.ReleaseCount))
        {
            _logger.LogInformation("Processing appId {AppId}", appId);
            
            SteamAppDetailsResponse? appDetailsResponse =
                await _service.GetAppDetails(appId, _settings.CountryCurrencyCode, _settings.Language);
            
            if (appDetailsResponse is null || !appDetailsResponse.Success)
            {
                _logger.LogWarning("Failed to fetch details for appId {AppId}", appId);

                continue;
            }

            bool anySent = await TrySendMessageToDiscordChannels(appId, settings, appDetailsResponse.Data!, skipGuildIds);
            if (!anySent)
            {
                break;
            }

            lastAppId ??= appId;

            await Task.Delay(10000);
        }

        if (lastAppId != null)
        {
            await using DbScope scope = _dbScopeProvider.GetDbScope();

            List<string> guildIds = settings.ConvertAll(x => x.GuildId.ToString());

            await _service.UpdateLastLoadedApp(lastAppId, guildIds, scope);
            
            _logger.LogInformation("AppId saved {AppId}", lastAppId);
        }
    }
    
    private async Task<List<SteamNewReleasesSettingsOrm>> GetGuildsSettings()
    {
        await using DbScope scope = _dbScopeProvider.GetDbScope();
        
        return await _service.GetSettings(scope)
            .Where(x => !x.Pause)
            .ToListAsync();
    }

    private async Task<bool> TrySendMessageToDiscordChannels(
        string appId,
        List<SteamNewReleasesSettingsOrm> settings,
        SteamAppDetails data,
        HashSet<string> skipGuildIds)
    {
        string? accompanyingAiMessage = null;

        var guildsToStopProcess = new HashSet<ulong>();
        bool needContinue = true;
        foreach (SteamNewReleasesSettingsOrm guildSettings in settings)
        {
            if (guildSettings.LastLoadedAppId == appId)
            {
                skipGuildIds.Add(guildSettings.GuildId);
            }

            if (settings.Count == skipGuildIds.Count)
            {
                needContinue = false;
                break;
            }

            if (skipGuildIds.Contains(guildSettings.GuildId))
            {
                continue;
            }

            if (!SettingsMatched(guildSettings, data))
            {
                continue;
            }

            if (_settings.EnableAccompanyingAiMessage && string.IsNullOrEmpty(accompanyingAiMessage))
            {
                accompanyingAiMessage = await GetAiMessage(data);
            }

            ulong guildId = ulong.Parse(guildSettings.GuildId);
            ulong channelId = ulong.Parse(guildSettings.ChannelId);

            bool sent = await TrySendMessageToDiscordChannel(guildId, channelId, appId, data, accompanyingAiMessage);
            if (!sent)
            {
                guildsToStopProcess.Add(guildId);
            }
        }

        if (guildsToStopProcess.Any())
        {
            await using DbScope scope = _dbScopeProvider.GetDbScope();
            await _service.TryPauseProcessOnGuilds(guildsToStopProcess, scope);   
        }

        return needContinue;
    }

    private bool SettingsMatched(SteamNewReleasesSettingsOrm guildSettings, SteamAppDetails data)
    {
        if (data.Type != "game")
        {
            return false;
        }

        if (_loadCategories.Any())
        {
            bool categoryMatched = data.Categories.Any(x => _loadCategories.Contains(x.Description));
            if (!categoryMatched)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> TrySendMessageToDiscordChannel(
        ulong guildId, 
        ulong channelId, 
        string appId, 
        SteamAppDetails data, 
        string? messageContent = null)
    {
        if (!_discordClient.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
        {
            _logger.LogWarning("Unable to find guild {GuildId}", guildId);
            return false;
        }

        if (!guild.Channels.TryGetValue(channelId, out DiscordChannel? channel))
        {
            _logger.LogWarning("Unable to find channel {ChannelId} in guild {GuildId}", channelId, guildId);
            return false;
        }

        DiscordEmbed embed = SteamNewReleasesLoaderDiscordEmbedBuilder.Build(appId, data);
        if (messageContent != null)
        {
            await channel.SendMessageAsync(messageContent, embed);   
        }
        else
        {
            await channel.SendMessageAsync(embed);   
        }
        
        return true;
    }
    
    private async Task<string?> GetAiMessage(SteamAppDetails appDetails)
    {
        try
        {
            var inputMessages = new List<ChatMessage>();
            
            inputMessages.Add(new SystemChatMessage(
                """
                Ты — игровой Discord-бот с чувством юмора и лёгким сарказмом. 
                Твоя задача — делать короткие, дружелюбные и слегка шутливые сообщения о новых играх, которые бот только что нашёл. 
                Начинай сообщение разнообразно. Добавляй немного сарказма, выделяй интересные особенности игры, делай текст кратким и емким. 
                Не повторяйся слишком часто, удивляй участников новым тоном.
                Будь краток.
                """
            ));
            
            string description = $"{appDetails.Name}: {appDetails.ShortDescription}\n" +
                                 $"Жанры: {string.Join(", ", appDetails.Genres.Select(g => g.Description))}\n" +
                                 $"Категории: {string.Join(", ", appDetails.Categories.Select(c => c.Description))}\n";

            inputMessages.Add(new UserChatMessage($"Новая игра найдена:\n{description}"));

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 150,
                Temperature = 0.7f,
                TopP = 0.9f,
                FrequencyPenalty = 0.2f,
                PresencePenalty = 0.3f
            };

            ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(inputMessages, options);

            string responseText = result.Value.Content[0].Text.Trim();
            return responseText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI message for game '{GameName}'", appDetails.Name);
            
            return null;
        }
    }
}