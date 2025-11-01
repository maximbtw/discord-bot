using System.Net.Http.Json;
using System.Text.Json;
using Bot.Commands.Checks.RequireApplicationOwner;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using HtmlAgilityPack;

namespace Bot.Commands.Commands;

public class TestCommand : ICommand
{
    [Command("test")]
    [MyRequireApplicationOwner]
    public async ValueTask Execute(CommandContext context)
    {
        await foreach (string appId in  GetLastAppIds())
        {
            SteamAppDetails? details = await GetAppDetails(appId);
            if (details is null)
                continue;
            
            // if (!details.Categories.Any(c => 
            //         c.Contains("Co-op", StringComparison.OrdinalIgnoreCase)))
            //     continue;

            // Формируем embed
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle(details.Name)
                .WithUrl($"https://store.steampowered.com/app/{appId}")
                .WithImageUrl(details.HeaderImage)
                .WithDescription($"🎮 **Жанры:** {string.Join(", ", details.Genres)}\n💰 **Цена:** {details.Price ?? "—"}")
                .WithColor(DiscordColor.Blurple);

            await context.Channel.SendMessageAsync(embed);

            await Task.Delay(1000); // чтобы не ловить бан
        }
    }

    private async IAsyncEnumerable<string> GetLastAppIds()
    {
        var http = new HttpClient();
        string url = "https://store.steampowered.com/search/results/?sort_by=Released_DESC&count=50&category1=998&infinite=1&cc=us&l=english";
        var json = await http.GetFromJsonAsync<JsonElement>(url);

        string html = json.GetProperty("results_html").GetString()!;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[contains(@class,'search_result_row')]");
        foreach (HtmlNode node in nodes)
        {
            string appId = node.GetAttributeValue("data-ds-appid", "");

            yield return appId;
        }
    }
    
    private async Task<SteamAppDetails?> GetAppDetails(string appId)
    {
        using var client = new HttpClient();
        var url = $"https://store.steampowered.com/api/appdetails?appids={appId}&cc=ru&l=russian";
        try
        {
            var json = await client.GetFromJsonAsync<JsonElement>(url);
            if (!json.TryGetProperty(appId, out var root) ||
                !root.TryGetProperty("data", out var data))
                return null;

            var name = data.GetProperty("name").GetString() ?? "Unknown";
            var headerImage = data.GetProperty("header_image").GetString() ?? "";
            var genres = data.TryGetProperty("genres", out var genresEl)
                ? genresEl.EnumerateArray().Select(x => x.GetProperty("description").GetString() ?? "").ToList()
                : new();
            var categories = data.TryGetProperty("categories", out var catEl)
                ? catEl.EnumerateArray().Select(x => x.GetProperty("description").GetString() ?? "").ToList()
                : new();

            string? price = null;
            if (data.TryGetProperty("price_overview", out var priceEl))
                price = priceEl.GetProperty("final_formatted").GetString();
            else if (data.TryGetProperty("is_free", out var freeEl) && freeEl.GetBoolean())
                price = "Бесплатно";

            return new SteamAppDetails
            {
                Name = name,
                HeaderImage = headerImage,
                Price = price ?? "N/A",
                Genres = genres,
                Categories = categories
            };
        }
        catch
        {
            return null;
        }
    }
    
    public class SteamAppDetails
    {
        public string Name { get; set; } = "";
        public string HeaderImage { get; set; } = "";
        public string? Price { get; set; }
        public List<string> Genres { get; set; } = new();
        public List<string> Categories { get; set; } = new();
    }
}