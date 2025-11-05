using System.Text.Json.Serialization;

namespace Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;

public class SteamAppDetails
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("short_description")]
    public string ShortDescription { get; set; } = string.Empty;

    [JsonPropertyName("header_image")]
    public string HeaderImage { get; set; } = string.Empty;
    
    [JsonPropertyName("is_free")]
    public bool IsFree { get; set; }
    
    [JsonPropertyName("price_overview")]
    public SteamAppPriceOverview? PriceOverview { get; set; }
    
    [JsonPropertyName("release_date")]
    public SteamAppReleaseDate? ReleaseDate { get; set; }
    
    [JsonPropertyName("genres")]
    public List<SteamAppGenre> Genres { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<SteamAppCategory> Categories { get; set; } = new();
    
    [JsonPropertyName("screenshots")]
    public List<SteamAppScreenshot> Screenshots { get; set; } = new();
}