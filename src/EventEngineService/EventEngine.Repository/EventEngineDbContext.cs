using EventEngine.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEngine.Repository;

public class EventEngineDbContext(DbContextOptions<EventEngineDbContext> dbContextOptions) : DbContext(dbContextOptions)
{

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.CallbackUrl).IsRequired();
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ReceivedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<DispatchLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            // association with Events
            entity.HasOne(d => d.Event)
                  .WithMany()
                  .HasForeignKey(d => d.EventId)
                  .OnDelete(DeleteBehavior.Cascade); // actually it's useless because it should not be possible to delete an Event

            entity.Property(e => e.DispatchedAt).HasDefaultValueSql("GETUTCDATE()");

            // associtation with Subscriptions
            entity.HasOne(d => d.Subscription)
                  .WithMany()
                  .HasForeignKey(d => d.SubscriptionId)
                  .OnDelete(DeleteBehavior.NoAction); // not deleting the logs if the subscription is deleted
        });

    }

    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<Event> Events { get; set; }

    public DbSet<DispatchLog> DispatchLogs { get; set; }
}