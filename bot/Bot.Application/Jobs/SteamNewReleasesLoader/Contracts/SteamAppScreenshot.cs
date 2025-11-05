using System.Text.Json.Serialization;

namespace Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;

public class SteamAppScreenshot
{
    [JsonPropertyName("path_thumbnail")]
    public string ThumbnailPath { get; set; } = string.Empty;
    
    [JsonPropertyName("path_full")]
    public string FullPath { get; set; } = string.Empty;
}
