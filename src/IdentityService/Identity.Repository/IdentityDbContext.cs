using Identity.Repository.Models;
using Microsoft.EntityFrameworkCore;
namespace Identity.Repository;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(x => x.UserId);
        modelBuilder.Entity<User>().HasKey(x => x.ApiToken);

    }

    public DbSet<User> Users { get; set; }
}