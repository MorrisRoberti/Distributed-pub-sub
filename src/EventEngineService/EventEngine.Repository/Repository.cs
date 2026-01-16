using System.Collections;
using EventEngine.Repository.Abstractions;
using EventEngine.Repository.Models;
using EventEngine.Shared;
using Microsoft.EntityFrameworkCore;
namespace EventEngine.Repository;

public class Repository(EventEngineDbContext eventEngineDbContext) : IRepository
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await eventEngineDbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<Event> CreateEventAsync(string EventType, string Payload, CancellationToken cancellationToken = default)
    {
        Event _event = new Event();
        _event.EventType = EventType;
        _event.Payload = Payload;

        await eventEngineDbContext.Events.AddAsync(_event, cancellationToken);
        return _event;
    }

    public async Task<Event?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await eventEngineDbContext.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpsertSubscriptionAsync(SubscriptionDTO subscription)
    {

        // i search the current Subscription to see if it already exists
        Subscription? existing = await eventEngineDbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscription.Id);

        if (existing == null)
        {
            // The Subscription does not exist i create it and add it to db
            Subscription newSub = new Subscription
            {
                // to be consistent I use the same id of the Subscription coming from the RegistryService
                Id = subscription.Id,
                EventType = subscription.EventType,
                CallbackUrl = subscription.CallbackUrl
            };
            eventEngineDbContext.Subscriptions.Add(newSub);
        }
        else
        {

            // here i know for sure that the subscription exists
            if (subscription.DeletedAt != null)
            {
                existing.DeletedAt = DateTime.UtcNow;
                existing.IsActive = false;

            }
            else
            {

                // the Subscription is already present, I update the necessary fields
                // NOTE: This is a Put so all fields should be populated
                existing.EventType = subscription.EventType;
                existing.CallbackUrl = subscription.CallbackUrl;
                existing.IsActive = subscription.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await eventEngineDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Event>> GetUnprocessedEventsAsync(CancellationToken cancellationToken)
    {
        return await eventEngineDbContext.Events
            .Where(e => !e.Processed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsFromEventTypeAsync(string eventType, CancellationToken cancellationToken)
    {
        return await eventEngineDbContext.Subscriptions
                .Where(s => s.EventType == eventType && s.IsActive && s.DeletedAt == null)
                .ToListAsync(cancellationToken);
    }

    public async Task<DispatchLog> CreateDispatchLogAsync(Guid eventId, Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        DispatchLog newLog = new DispatchLog
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            SubscriptionId = subscriptionId,
            Status = "PENDING", // actually PENDING is already the default value, but i leave it for clarity's sake
            Attempts = 0, // same as above
            DispatchedAt = DateTime.UtcNow
        };

        await eventEngineDbContext.DispatchLogs.AddAsync(newLog, cancellationToken);
        return newLog;
    }

    public async Task<IEnumerable<DispatchLog>> GetPendingDispatchLogsAsync(CancellationToken cancellationToken)
    {
        return await eventEngineDbContext.DispatchLogs
             .Include(l => l.Event)
             .Where(l => l.Status == "PENDING" || l.Status == "FAILED" && l.Attempts < 3)
             .ToListAsync(cancellationToken);
    }

    public async Task<string> GetCallbackUrlOfSubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        return await eventEngineDbContext.Subscriptions
                .Where(s => s.Id == subscriptionId)
                .Select(s => s.CallbackUrl)
                .FirstOrDefaultAsync(cancellationToken);
    }
}