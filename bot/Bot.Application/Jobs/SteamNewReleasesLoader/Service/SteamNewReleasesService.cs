using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;
using Bot.Domain.Orms.SteamNewReleasesSettings;
using Bot.Domain.Scope;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace Bot.Application.Jobs.SteamNewReleasesLoader.Service;

internal class SteamNewReleasesService : ISteamNewReleasesService
{
    private const string SteamSearchBaseUrl = "https://store.steampowered.com/search/results/";
    private const string SteamAppDetailsBaseUrl = "https://store.steampowered.com/api/appdetails";

    private const string DefaultLanguage = "english";
    private const string DefaultCurrency = "US";

    private readonly ISteamNewReleasesSettingsRepository _settingsRepository;
    
    public SteamNewReleasesService(ISteamNewReleasesSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async IAsyncEnumerable<string> GetLastAppIds(
        int releaseCount, 
        string? cc = null, 
        string? language = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        string url = BuildSearchUrl();

        var httpClient = new HttpClient();
        var json = await httpClient.GetFromJsonAsync<JsonElement>(url, ct);

        if (!json.TryGetProperty("results_html", out JsonElement resultsHtml))
        {
            yield break;
        }

        string html = resultsHtml.GetString()!;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[contains(@class,'search_result_row')]");

        foreach (HtmlNode node in nodes)
        {
            string appId = node.GetAttributeValue("data-ds-appid", "");
            if (!string.IsNullOrWhiteSpace(appId))
            {
                yield return appId;
            }
        }

        yield break;

        string BuildSearchUrl()
        {
            cc ??= DefaultCurrency;
            language ??= DefaultLanguage;

            return
                $"{SteamSearchBaseUrl}?sort_by=Released_DESC&count={releaseCount}&category1=998&infinite=1&cc={cc}&l={language}";
        }
    }

    public async Task<SteamAppDetailsResponse?> GetAppDetails(
        string appId, 
        string? cc = null, 
        string? language = null, 
        CancellationToken ct = default)
    {
        string url = BuildSearchUrl();

        var httpClient = new HttpClient();
            
        string response = await httpClient.GetStringAsync(url, ct);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        using JsonDocument document = JsonDocument.Parse(response);
        if (!document.RootElement.TryGetProperty(appId, out JsonElement jsonElement))
        {
            return null;
        }

        var appDetails = JsonSerializer.Deserialize<SteamAppDetailsResponse>(jsonElement.GetRawText());

        return appDetails;
        
        string BuildSearchUrl()
        {
            cc ??= DefaultCurrency;
            language ??= DefaultLanguage;

            return $"{SteamAppDetailsBaseUrl}?appids={appId}&cc={cc}&l={language}";
        }
    }

    public IQueryable<SteamNewReleasesSettingsOrm> GetSettings(DbScope scope)
    {
        return _settingsRepository.GetQueryable(scope);
    }

    public async Task AddOrUpdateGuildSettings(ulong guildId, ulong channelId, DbScope scope, CancellationToken ct = default)
    {
        SteamNewReleasesSettingsOrm? orm = await _settingsRepository
            .GetUpdateQueryable(scope)
            .FirstOrDefaultAsync(x => x.GuildId == guildId.ToString(), ct);
        
        if (orm is null)
        {
            orm = new SteamNewReleasesSettingsOrm
            {
                GuildId = guildId.ToString(),
                ChannelId = channelId.ToString(),
            };

            await _settingsRepository.Insert(orm, scope, ct);
        }
        else
        {
            orm.ChannelId = channelId.ToString();
        }

        await scope.CommitAsync(ct);
    }
    
    public async Task<bool> TryPauseProcessOnGuilds(IEnumerable<ulong> guildIds, DbScope scope, CancellationToken ct = default)
    {
        List<string> ids  = guildIds.Select(x=>x.ToString()).ToList();

        List<SteamNewReleasesSettingsOrm> orms = await _settingsRepository
            .GetUpdateQueryable(scope)
            .Where(x => ids.Contains(x.GuildId))
            .Where(x => !x.Pause)
            .ToListAsync(ct);

        if (orms.Any())
        {
            foreach (SteamNewReleasesSettingsOrm orm in orms)
            {
                orm.Pause = true;
            }

            await scope.CommitAsync(ct);

            return true;
        }

        return false;
    }

    public async Task UpdateLastLoadedApp(string appId, List<string> guildIds, DbScope scope, CancellationToken ct = default)
    {
        DateTime now = DateTime.UtcNow;
        
        List<SteamNewReleasesSettingsOrm> orms = await _settingsRepository
            .GetUpdateQueryable(scope)
            .Where(x => guildIds.Contains(x.GuildId))
            .ToListAsync(ct);

        foreach (SteamNewReleasesSettingsOrm orm in orms)
        {
            orm.LastLoadedAppId = appId;
            orm.LastLoadedAppDateTime = now;
        }

        await scope.CommitAsync(ct);
    }
}