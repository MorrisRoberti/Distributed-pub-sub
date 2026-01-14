using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEngine.Repository.Models;

[Table("Events")]
public class Event
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string EventType { get; set; } = string.Empty;
    [Required]
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
}