using Registry.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Registry.Repository;

public class SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>().HasKey(x => x.Id);
        modelBuilder.Entity<Subscription>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Subscription>().Property(e => e.CreatedAt).ValueGeneratedOnAdd();
    }

    public DbSet<Subscription> Subscriptions { get; set; }
}