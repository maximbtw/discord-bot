using System.ComponentModel.DataAnnotations;
using Bot.Application.Handlers.Chat.OpenAiImpersonationChat;
using Bot.Application.Handlers.Chat.OpenAiSimpleChat;

namespace Bot.Application.Infrastructure.Configuration;

public class ChatOptions
{
    public OpenAiSimpleChatOptions SimpleChatOptions { get; set; } = null!;
    
    public OpenAiImpersonationChatOptions ImpersonationChatOptions { get; set; } = null!;
    
    public string? BadRequestMessage { get; set; } = null!;
    
    public int TimeOutInSeconds { get; set; } = 30;
    
    [Required]
    public string Model { get; set; } = string.Empty;
    
    [Range(0, 100)]
    public int RandomMessageChance { get; set; } 
}