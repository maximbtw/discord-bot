using System.ComponentModel.DataAnnotations;
using Bot.Application.Chat;

namespace Bot.Application.Infrastructure.Configuration;

public class OpenAiSettings
{
    [Required] public string ApiKey { get; set; } = string.Empty;

    public bool UseOpenRouter { get; set; }
    
    public string Model { get; set; } = string.Empty;
}