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
using Quartz;

namespace Bot.Application.Jobs.SteamNewReleasesLoader;

[DisallowConcurrentExecution]
internal class SteamNewReleasesLoaderJob : IJob
{
    private readonly ISteamNewReleasesService _service;
    private readonly DiscordClient _discordClient ;
    private readonly SteamNewReleasesLoaderSettings _settings;
    private readonly IDbScopeProvider _dbScopeProvider;
    private readonly ILogger<SteamNewReleasesLoaderJob> _logger;
    
    private static readonly FrozenSet<string> MultiplayerCategories = new[]
    {
        "Multi-player",
        "Online PvP",
        "LAN PvP",
        "Shared/Split Screen PvP",
        "Online Co-op",
        "Shared/Split Screen Co-op",
        "Co-op",
        "Cross-Platform Multiplayer",
        "MMO",
        "PvP",
        "Local Multiplayer",
        "Online Multiplayer"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public SteamNewReleasesLoaderJob(
        ISteamNewReleasesService service, 
        DiscordClient discordClient,
        IConfiguration configuration, 
        IDbScopeProvider dbScopeProvider, 
        ILogger<SteamNewReleasesLoaderJob> logger)
    {
        _service = service; 
        _discordClient = discordClient;
        _dbScopeProvider = dbScopeProvider;
        _logger = logger;
        _settings =  configuration.GetSection(nameof(SteamNewReleasesLoaderSettings)).Get<SteamNewReleasesLoaderSettings>()!;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await ExecuteCore();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error executing SteamNewReleasesLoaderJob");
        }
    }

    private async Task ExecuteCore()
    {
        List<SteamNewReleasesSettingsOrm> settings = await GetGuildsSettings();
        if (!settings.Any())
        {
            return;
        }

        var skipGuildIds = new HashSet<string>();
        string? lasAppId = null;
        await foreach (string appId in _service.GetLastAppIds(_settings.ReleaseCount))
        {
            SteamAppDetailsResponse? appDetailsResponse =
                await _service.GetAppDetails(appId, _settings.CountryCurrencyCode, _settings.Language);
            
            if (appDetailsResponse is null || !appDetailsResponse.Success)
            {
                // TODO: Log

                continue;
            }

            bool anySent = await TrySendMessageToDiscordChannels(appId, settings, appDetailsResponse.Data!, skipGuildIds);
            if (!anySent)
            {
                break;
            }

            lasAppId ??= appId;

            await Task.Delay(2000);
        }

        if (lasAppId != null)
        {
            await using DbScope scope = _dbScopeProvider.GetDbScope();

            List<string> guildIds = settings.ConvertAll(x => x.GuildId.ToString());

            await _service.UpdateLastLoadedApp(lasAppId, guildIds, scope);
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
        bool anySent = false;
        foreach (SteamNewReleasesSettingsOrm guildSettings in settings)
        {
            if (!SettingsMatched(guildSettings, data))
            {
                continue;
            }

            if (guildSettings.LastLoadedAppId == appId)
            {
                skipGuildIds.Add(guildSettings.GuildId);
            }

            if (skipGuildIds.Contains(guildSettings.GuildId))
            {
                continue;
            }
            
            anySent = true;

            ulong guildId = ulong.Parse(guildSettings.GuildId);
            ulong channelId = ulong.Parse(guildSettings.ChannelId);

            await SendMessageToDiscordChannel(guildId, channelId, appId, data);
        }

        return anySent;
    }
    
    private bool SettingsMatched(SteamNewReleasesSettingsOrm guildSettings, SteamAppDetails data)
    {
        if (data.Type != "game")
        {
            return false;
        }

        // TODO: В будущем можно будет добавить настроку категорий через бота.
        bool isMultiplayer = data.Categories.Any(x => MultiplayerCategories.Contains(x.Description));
        if (!isMultiplayer)
        {
            return false;
        }

        return true;
    }

    private async Task SendMessageToDiscordChannel(ulong guildId, ulong channelId, string appId,  SteamAppDetails data)
    {
        // TODO: Если канал или сервер удалили?
        DiscordGuild guild = await _discordClient.GetGuildAsync(guildId);
        DiscordChannel channel = await guild.GetChannelAsync(channelId);

        DiscordEmbed embed = SteamNewReleasesLoaderDiscordEmbedBuilder.Build(appId, data);
        
        await channel.SendMessageAsync(embed);
    }
}