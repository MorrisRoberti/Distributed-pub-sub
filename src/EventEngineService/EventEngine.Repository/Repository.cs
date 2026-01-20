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
        return await eventEngineDbContext.Events.Where(e => e.Id == eventId).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task UpsertSubscriptionAsync(SubscriptionDTO subscription, CancellationToken cancellationToken = default)
    {

        // Search the current Subscription to see if it already exists
        Subscription? existing = await eventEngineDbContext.Subscriptions.FindAsync(subscription.Id, cancellationToken);

        if (existing is null)
        {
            // The Subscription does not exist i create it and add it to db
            Subscription newSub = new Subscription
            {
                // To be consistent I use the same id of the Subscription coming from the RegistryService
                Id = subscription.Id,
                EventType = subscription.EventType,
                CallbackUrl = subscription.CallbackUrl
            };
            eventEngineDbContext.Subscriptions.Add(newSub);
        }
        else
        {

            // Here i know for sure that the subscription exists
            // I check if it is deleted or inactive
            if (subscription.DeletedAt is not null)
            {
                existing.DeletedAt = DateTime.UtcNow;
                existing.IsActive = false;

            }
            else
            {

                // The Subscription is present and active (and not deleted), I update the necessary fields
                // NOTE: This is a Put so all fields should be populated
                existing.EventType = subscription.EventType;
                existing.CallbackUrl = subscription.CallbackUrl;
                existing.IsActive = subscription.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await eventEngineDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetUnprocessedEventsAsync(CancellationToken cancellationToken = default)
    {
        return await eventEngineDbContext.Events
            .Where(e => !e.Processed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsFromEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
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

    // Takes every PENDING or FAILED DispatchLog that has had less than 3 attempts
    public async Task<IEnumerable<DispatchLog>> GetPendingDispatchLogsAsync(CancellationToken cancellationToken = default)
    {
        return await eventEngineDbContext.DispatchLogs
             .Include(l => l.Event)
             .Where(l => l.Status == "PENDING" || l.Status == "FAILED" && l.Attempts < 3)
             .ToListAsync(cancellationToken);
    }

    public async Task<string?> GetCallbackUrlOfSubscription(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await eventEngineDbContext.Subscriptions
                .Where(s => s.Id == subscriptionId)
                .Select(s => s.CallbackUrl)
                .SingleOrDefaultAsync(cancellationToken);
    }
}