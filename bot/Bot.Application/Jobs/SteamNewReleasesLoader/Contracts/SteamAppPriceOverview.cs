using System.Text.Json.Serialization;

namespace Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;

public class SteamAppPriceOverview
{
    [JsonPropertyName("discount_percent")]
    public int DiscountPercent { get; set; }
    
    [JsonPropertyName("initial_formatted")]
    public string InitialFormatted { get; set; }  = string.Empty;
    
    [JsonPropertyName("final_formatted")]
    public string FinalFormatted { get; set; }  = string.Empty;
}