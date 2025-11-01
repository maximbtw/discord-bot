using System.ComponentModel.DataAnnotations;

namespace Bot.Application.Jobs.SteamNewReleasesLoader;

public class SteamNewReleasesLoaderSettings
{
    public bool Enabled { get; set; } = true;
    
    [Required]
    public int IntervalInMinutes  { get; set; }
    
    [Required]
    public int ReleaseCount { get; set; }

    public List<string> LoadCategories { get; set; } = new();
    
    public string? CountryCurrencyCode { get; set; }
    
    public string? Language { get; set; }
}