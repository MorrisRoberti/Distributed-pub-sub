using EventEngine.Repository.Models;
using EventEngine.Shared;
namespace EventEngine.Repository.Abstractions;

public interface IRepository
{

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Event> CreateEventAsync(string EventType, string Payload, CancellationToken cancellationToken = default);
    Task<Event?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task UpsertSubscriptionAsync(SubscriptionDTO dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetUnprocessedEventsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetSubscriptionsFromEventTypeAsync(string eventType, CancellationToken cancellationToken = default);
    Task<DispatchLog> CreateDispatchLogAsync(Guid eventId, Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DispatchLog>> GetPendingDispatchLogsAsync(CancellationToken cancellationToken = default);
    Task<string?> GetCallbackUrlOfSubscription(Guid subscriptionId, CancellationToken cancellationToken = default);
}