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

        modelBuilder.Entity<OutboxMessage>()
        .HasKey(e => e.Id);
        modelBuilder.Entity<OutboxMessage>().Property(e => e.Id).ValueGeneratedOnAdd();
    }

    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}