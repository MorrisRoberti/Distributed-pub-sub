using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Repository.Models;

[Table("Users")]
public class User
{
    // To create a composite Key EF Core forces you to use the Fluent API, so I removed the [Key] annotation
    public string UserId { get; set; } = string.Empty;

    public string ApiToken { get; set; } = string.Empty;

    public DateTime TokenExpireDate { get; set; }

    public int RemainingRequests { get; set; }
}