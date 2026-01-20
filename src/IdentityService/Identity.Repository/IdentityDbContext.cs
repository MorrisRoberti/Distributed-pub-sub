using Identity.Repository.Models;
using Microsoft.EntityFrameworkCore;
namespace Identity.Repository;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ATTENTION: before, I did something like ...HasKey(x => x.UserId); ...HasKey(x => x.ApiToken)
        // this is wrong because it replaces the first key with the second, it doesn't create a composite key
        modelBuilder.Entity<User>().HasKey(x => new { x.UserId, x.ApiToken });

    }

    public DbSet<User> Users { get; set; }
}