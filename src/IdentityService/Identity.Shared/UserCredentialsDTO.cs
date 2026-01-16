using System.ComponentModel.DataAnnotations;

namespace Identity.Shared;

public class UserCredentialsDTO
{
    [Required]
    public Guid UserId { get; set; }

    public string? ApiToken { get; set; }

}
