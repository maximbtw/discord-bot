using System.ComponentModel.DataAnnotations;
using Bot.Domain.Orms.ChatSettings;

namespace Bot.Contracts.Chat;

public class GuildChatSettings
{
    public ulong GuildId { get; set; } 
    
    public ChatType ChatType { get; set; } = ChatType.Default;
    
    [Range(0, 100)]
    public int ResponseChance { get; set; } 
    
    [Range(1, 40)]
    public int ChatHistoryLimit { get; set; }
    
    public bool ReplaceMentions { get; set; }
    
    [MaxLength(20)]
    public ulong? ImpersonationUserId { get; set; }
}