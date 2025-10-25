using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.Message;

[Table("Messages")]
public class MessageOrm
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(20)]
    public string Id { get; set; }  = string.Empty;
    
    [MaxLength(20)]
    public string UserId { get; set; }  = string.Empty;
    
    [MaxLength(32)]
    public string UserNickname { get; set; } = string.Empty;
    
    public bool UserIsBot { get; set; }
    
    [MaxLength(20)]
    public string ChannelId { get; set; }  = string.Empty;
    
    [MaxLength(20)]
    public string GuildId { get; set; }  = string.Empty;

    [MaxLength(10000)]
    public string Content { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    [MaxLength(20)]
    public string? ReplyToMessageId { get; set; }

    [MaxLength(10000)]
    public string? MentionedUserIdsJson { get; set; }

    public bool HasAttachments { get; set; }
    
    [NotMapped]
    public List<string> MentionedUserIds
    {
        get => string.IsNullOrEmpty(MentionedUserIdsJson)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(MentionedUserIdsJson)!;

        init => MentionedUserIdsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}