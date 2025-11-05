using System.Text.Json.Serialization;

namespace Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;

public class SteamAppReleaseDate
{
    [JsonPropertyName("coming_soon")]
    public bool ComingSoon { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
}