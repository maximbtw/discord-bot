using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.Orms.SteamNewReleasesSettings;

[Table("SteamNewReleasesSettings")]
public class SteamNewReleasesSettingsOrm
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(20)]
    public string GuildId { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string ChannelId { get; set; } = string.Empty;
    
    public bool Pause {get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(10)]
    public string? LastLoadedAppId { get; set; }
    
    public DateTime? LastLoadedAppDateTime { get; set; }
}