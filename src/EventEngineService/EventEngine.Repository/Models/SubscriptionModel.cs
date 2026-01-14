using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEngine.Repository.Models;

[Table("Subscriptions")]
public class Subscription
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public string CallbackUrl { get; set; } = string.Empty;
}