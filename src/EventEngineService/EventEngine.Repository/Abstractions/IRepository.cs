using EventEngine.Repository.Models;
namespace EventEngine.Repository.Abstractions;

public interface IRepository
{

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Event> CreateEventAsync(string EventType, string Payload, CancellationToken cancellationToken = default);

    Task<Event?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}