using EventEngine.Shared;
namespace EventEngine.Business.Abstractions;

public interface IBusiness
{
    Task<Guid> CreateEventAsync(EventDTO _event, CancellationToken cancellationToken = default);

    Task<EventDTO?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}