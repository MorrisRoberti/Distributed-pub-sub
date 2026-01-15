using Registry.Repository.Abstractions;
using Registry.Repository.Models;
using Registry.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;
namespace Registry.Repository;

public class Repository(SubscriptionDbContext subscriptionDbContext) : IRepository
{

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await subscriptionDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await subscriptionDbContext.Database.BeginTransactionAsync();
    }

    public async Task AddOutboxMessageAsync(SubscriptionDTO subscription)
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "SubscriptionCreated",
            Payload = JsonSerializer.Serialize(subscription),
            OccurredOnUtc = DateTime.UtcNow
        };

        await subscriptionDbContext.OutboxMessages.AddAsync(outboxMessage);
    }
    public async Task<IEnumerable<OutboxMessage>> GetPendingOutboxMessagesAsync()
    {
        return await subscriptionDbContext.Set<OutboxMessage>()
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .ToListAsync();
    }

    public async Task MarkOutboxMessageAsProcessedAsync(Guid id)
    {
        var message = await subscriptionDbContext.Set<OutboxMessage>().FindAsync(id);
        if (message != null)
        {
            message.ProcessedOnUtc = DateTime.UtcNow;
            await subscriptionDbContext.SaveChangesAsync();
        }
    }
    public async Task<Subscription> CreateSubscriptionAsync(string UserId, string EventType, string CallbackUrl, CancellationToken cancellationToken = default)
    {

        Subscription sub = new Subscription();
        sub.UserId = UserId;
        sub.EventType = EventType;
        sub.CallbackUrl = CallbackUrl;
        sub.IsActive = true;

        await subscriptionDbContext.Subscriptions.AddAsync(sub, cancellationToken);
        return sub;
    }

    public async Task<Subscription?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await subscriptionDbContext.Subscriptions.Where(s => s.Id == subscriptionId).FirstOrDefaultAsync(cancellationToken);
    }
    public void UpdateSubscription(Subscription subscription)
    {
        subscriptionDbContext.Subscriptions.Update(subscription);
    }
    public void DeleteSubscription(Subscription subscription)
    {
        subscriptionDbContext.Subscriptions.Remove(subscription);
    }
}
