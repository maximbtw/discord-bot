using System.Text.Json.Serialization;

namespace Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;

public class SteamAppGenre
{
    [JsonPropertyName("description")]
    public string Description { get; set; }  = string.Empty;
}
