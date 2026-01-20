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

    public async Task AddOutboxMessageAsync(Subscription subscription, string operation)
    {

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            Type = $"Subscription{operation}",
            Payload = JsonSerializer.Serialize(subscription),
            OccurredOnUtc = DateTime.UtcNow
        };

        // If this fails the exception gets caught at the business level to rollback the transaction
        await subscriptionDbContext.OutboxMessages.AddAsync(outboxMessage);
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingOutboxMessagesAsync()
    {
        return await subscriptionDbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null) // Search all the non-processed messages
            .OrderBy(x => x.OccurredOnUtc) // It's important to get the messages in the order in which they have been sent
            .ToListAsync();
    }

    public async Task MarkOutboxMessageAsProcessedAsync(Guid id)
    {
        var message = await subscriptionDbContext.OutboxMessages.FindAsync(id);
        if (message is not null)
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

        // If this fails the exception gets caught at the business level to rollback the transaction
        await subscriptionDbContext.Subscriptions.AddAsync(sub, cancellationToken);
        return sub;
    }

    public async Task<Subscription?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        // I use SingleOrDefaultAsync because it gives an error if the record is duplicated
        return await subscriptionDbContext.Subscriptions.Where(s => s.Id == subscriptionId && s.DeletedAt == null).SingleOrDefaultAsync(cancellationToken);
    }
    public void UpdateSubscription(Subscription subscription)
    {
        subscriptionDbContext.Subscriptions.Update(subscription);
    }

}
