using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEngine.Repository.Models;

[Table("Subscriptions")]
public class Subscription
{
    [Key]
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;

    [Required]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public string CallbackUrl { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }
}