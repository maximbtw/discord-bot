using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.Orms.ChatSettings;

[Table("ChatSettings")]
public class ChatSettingsOrm : IOrm
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(20)]
    public string GuildId { get; set; } = string.Empty;
    
    public ChatType ChatType { get; set; } = ChatType.Default;
    
    [Range(0, 100)]
    public int? ResponseChance { get; set; } 
    
    [Range(1, 40)]
    public int? ChatHistoryLimit { get; set; }
    
    public bool ReplaceMentions { get; set; }
    
    [MaxLength(20)]
    public string? ImpersonationUserId { get; set; }
}