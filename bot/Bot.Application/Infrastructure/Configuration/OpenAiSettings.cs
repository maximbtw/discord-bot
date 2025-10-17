using System.ComponentModel.DataAnnotations;

namespace Bot.Application.Infrastructure.Configuration;

public class OpenAiSettings
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;
    
    public bool UseOpenRouter { get; set; }

    public ChatOptions ChatOptions { get; set; } = null!;
}