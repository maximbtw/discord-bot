using System.ComponentModel.DataAnnotations;

namespace Bot.Application.Infrastructure.Configuration;

public class BotConfiguration
{
    [Required]
    public DatabaseOptions DatabaseOptions { get; set; } = null!;
    
    [Required]
    public DiscordOptions DiscordOptions { get; set; } = null!;

    [Required]
    public string DatasetFilePath { get; set; } = string.Empty;

    [Required]
    public string TrainingServerUrl { get; set; } = string.Empty;
}