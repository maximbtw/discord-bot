using System.ComponentModel.DataAnnotations;

namespace Bot.Application.Infrastructure.Configuration;

public class DiscordOptions
{
    [Required]
    public string Token { get; set; } = string.Empty;
        
    [Required]
    public string Prefix { get;  set; } = string.Empty;
}