using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Registry.Repository.Models;

[Table("OutboxMessages")]
public class OutboxMessage
{
    [Key]
    public Guid Id { get; set; }

    public Guid SubscriptionId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Payload { get; set; } = string.Empty;

    [Required]
    public DateTime OccurredOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }
}