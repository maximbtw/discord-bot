using System.ComponentModel.DataAnnotations;
using Bot.Application.Chat.OpenAiImpersonationChat;
using Bot.Application.Chat.OpenAiSimpleChat;

namespace Bot.Application.Chat;

public class ChatSettings
{
    public OpenAiSimpleChatOptions SimpleChatOptions { get; set; } = null!;
    
    public OpenAiImpersonationChatOptions ImpersonationChatOptions { get; set; } = null!;
    
    public string? BadRequestMessage { get; set; } = null!;
    
    public int TimeOutInSeconds { get; set; } = 30;
    
    [Required]
    public string Model { get; set; } = string.Empty;
    
    [Range(0, 100)]
    public int RandomMessageChance { get; set; } 
    
    public ChatType DefaultStrategy { get; set; }
}