using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.Message;

[Table("Messages")]
public class MessageOrm
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; } 
    
    public long UserId { get; set; } 
    
    public string UserName { get; set; } = default!;
    
    public bool UserIsBot { get; set; }
    
    public long ChannelId { get; set; } 
    
    public long ServerId { get; set; } 

    public string? Content { get; set; }

    public DateTime Timestamp { get; set; }

    public long? ReplyToMessageId { get; set; }
    
    public string? MentionedUserIdsJson { get; set; }

    public bool HasAttachments { get; set; }
    
    [NotMapped]
    public List<long> MentionedUserIds
    {
        get => string.IsNullOrEmpty(MentionedUserIdsJson)
            ? new List<long>()
            : System.Text.Json.JsonSerializer.Deserialize<List<long>>(MentionedUserIdsJson)!;

        set => MentionedUserIdsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}