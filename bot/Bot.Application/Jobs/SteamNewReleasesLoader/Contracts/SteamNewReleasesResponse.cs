using System.Text.Json.Serialization;

namespace Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;

public class SteamAppDetailsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public SteamAppDetails? Data { get; set; }
}

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

public class SteamAppPriceOverview
{
    [JsonPropertyName("discount_percent")]
    public int DiscountPercent { get; set; }
    
    [JsonPropertyName("initial_formatted")]
    public string InitialFormatted { get; set; }  = string.Empty;
    
    [JsonPropertyName("final_formatted")]
    public string FinalFormatted { get; set; }  = string.Empty;
}

public class SteamAppGenre
{
    [JsonPropertyName("description")]
    public string Description { get; set; }  = string.Empty;
}

public class SteamAppCategory
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class SteamAppReleaseDate
{
    [JsonPropertyName("coming_soon")]
    public bool ComingSoon { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
}

public class SteamAppScreenshot
{
    [JsonPropertyName("path_thumbnail")]
    public string ThumbnailPath { get; set; } = string.Empty;
    
    [JsonPropertyName("path_full")]
    public string FullPath { get; set; } = string.Empty;
}
