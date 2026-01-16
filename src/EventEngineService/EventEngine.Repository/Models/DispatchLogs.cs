using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEngine.Repository.Models;

[Table("DispatchLogs")]
public class DispatchLog
{
    [Key]
    public Guid Id { get; set; }
    public Guid EventId { get; set; }

    [ForeignKey("ReceivedEventId")]
    public virtual Event? Event { get; set; }

    public Guid SubscriptionId { get; set; }

    [ForeignKey("SubscriptionId")]
    public virtual Subscription? Subscription { get; set; }
    public string Status { get; set; } = "PENDING";
    public int Attempts { get; set; } = 0;
    public DateTime DispatchedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}