using System.ComponentModel.DataAnnotations;
using Bot.Domain.Configuration;

namespace Bot.Application.Infrastructure.Configuration;

public class BotConfiguration
{
    [Required] 
    public string Token { get; set; } = string.Empty;
        
    [Required]
    public string Prefix { get;  set; } = string.Empty;
    
    [Required]
    public string AdminUsername { get;  set; } = string.Empty;
    
    public DatabaseOptions DatabaseOptions { get; set; } = null!;
}