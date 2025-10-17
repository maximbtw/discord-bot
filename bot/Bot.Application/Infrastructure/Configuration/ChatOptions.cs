using System.ComponentModel.DataAnnotations;

namespace Bot.Application.Infrastructure.Configuration;

public class ChatOptions
{
    [Required]
    public string Model { get; set; } = string.Empty;

    public string? SystemMessage { get; set; } = null!;
    
    public string? BadRequestMessage { get; set; } = null!;

    public int MaxChatHistoryMessages { get; set; } = 20;
    
    public int MaxOutputTokenCount { get; set; }
    public int MaxInputTokenCount { get; set; }

    public int TimeOutInSeconds { get; set; } = 30;
    
    [Range(0, 100)]
    public int RandomMessageChance { get; set; } 
}