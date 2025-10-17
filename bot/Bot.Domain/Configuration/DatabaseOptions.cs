using System.ComponentModel.DataAnnotations;

namespace Bot.Domain.Configuration;

public class DatabaseOptions
{
    public bool UseInMemoryDatabase { get; set; } 
    
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}