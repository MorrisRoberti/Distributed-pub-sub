namespace EventEngine.Shared;

public class SubscriptionDTO
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}