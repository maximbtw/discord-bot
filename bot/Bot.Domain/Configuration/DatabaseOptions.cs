using System.ComponentModel.DataAnnotations;

namespace Bot.Domain.Configuration;

public class DatabaseOptions
{
    public bool UseDb { get; set; } = true;
    
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}