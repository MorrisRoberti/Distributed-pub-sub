using Registry.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Registry.Repository;

public class SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>().HasKey(s => s.Id);
        modelBuilder.Entity<Subscription>().Property(s => s.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Subscription>().Property(s => s.CreatedAt).ValueGeneratedOnAdd();

        modelBuilder.Entity<OutboxMessage>().HasKey(m => m.Id);
        modelBuilder.Entity<OutboxMessage>().Property(m => m.Id).ValueGeneratedOnAdd();
        // could add a ValueGeneratedOnAdd also for OccurredOnUtc field
    }

    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}