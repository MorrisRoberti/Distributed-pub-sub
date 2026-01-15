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
            .FirstOrDefaultAsync(s => s.Id == subscription.Id && s.DeletedAt == null);

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
}