using System.ComponentModel.DataAnnotations;
using Bot.Application.Chat.DefaultChat;
using Bot.Application.Chat.ImpersonationChat;
using Bot.Domain.Orms.ChatSettings;

namespace Bot.Application.Chat;

public class ChatSettings
{
    public bool Enabled { get; set; }
    
    public ChatType DefaultChatType { get; set; }
    
    [Range(0, 100)]
    public int DefaultResponseChance { get; set; } 
    
    [Range(1, 40)]
    public int DefaultChatHistoryLimit { get; set; }
    
    public DefaultChatOptions DefaultChatOptions { get; set; } = null!;
    
    public ImpersonationChatOptions ImpersonationChatOptions { get; set; } = null!;
    
    public string? BadRequestMessage { get; set; } = null!;
    
    public int TimeOutInSeconds { get; set; } = 30;
    
    public int? MaxOutputTokenCount { get; set; } = 200;

    public float? Temperature { get; set; } = 0.7f;

    public float? TopP { get; set; } = 0.9f;

    public float? FrequencyPenalty { get; set; } = 0.2f;

    public float? PresencePenalty { get; set; } = 0.3f;
}