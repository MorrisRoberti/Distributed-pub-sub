using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Repository.Models;

[Table("Users")]
public class User
{
    [Key]
    public Guid UserId { get; set; }

    [Key]
    public string ApiToken { get; set; }

    public DateTime TokenExpireDate { get; set; }

    public int RemainingRequests { get; set; }
}