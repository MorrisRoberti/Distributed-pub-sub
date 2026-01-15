using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Registry.Shared;

public class SubscriptionDTO
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public string UserId { get; set; } = string.Empty;


    [Required]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public string CallbackUrl { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; } = true;
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}