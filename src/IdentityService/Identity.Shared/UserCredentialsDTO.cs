using System.ComponentModel.DataAnnotations;

namespace Identity.Shared;

public class UserCredentialsDTO
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string? ApiToken { get; set; }

}
