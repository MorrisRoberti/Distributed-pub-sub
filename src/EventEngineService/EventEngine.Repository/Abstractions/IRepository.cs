using EventEngine.Repository.Models;
using EventEngine.Shared;
namespace EventEngine.Repository.Abstractions;

public interface IRepository
{

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Event> CreateEventAsync(string EventType, string Payload, CancellationToken cancellationToken = default);
    Task<Event?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task UpsertSubscriptionAsync(SubscriptionDTO dto);
    Task<IEnumerable<Event>> GetUnprocessedEventsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Subscription>> GetSubscriptionsFromEventTypeAsync(string eventType, CancellationToken cancellationToken);
    Task<DispatchLog> CreateDispatchLogAsync(Guid eventId, CancellationToken cancellationToken = default);
}