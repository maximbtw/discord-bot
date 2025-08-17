using System.ComponentModel.DataAnnotations;
using Bot.Application.Infrastructure.Configuration.AiChat;

namespace Bot.Application.Infrastructure.Configuration;

public class BotConfiguration
{
    [Required] 
    public string Token { get; set; } = string.Empty;
        
    [Required]
    public string Prefix { get;  set; } = string.Empty;
    
    [Required]
    public DatabaseOptions DatabaseOptions { get; set; } = null!;
    
    [Required]
    public AiChatOptions AiChatOptions { get; set; } = null!;
}