using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using EventEngine.Shared;
using EventEngine.Repository.Abstractions;
using EventEngine.Repository.Models;
using EventEngine.Business.Abstractions;
namespace EventEngine.Business;

public class Business(IRepository repository, ILogger<Business> logger) : IBusiness
{
    public async Task<Guid> CreateEventAsync(EventDTO _event, CancellationToken cancellationToken = default)
    {
        Event evCreated = await repository.CreateEventAsync(_event.EventType, _event.Payload, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return evCreated.Id;
    }

    public async Task<EventDTO?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var _event = await repository.GetEventAsync(eventId);

        if (_event is null)
            return null;

        return new EventDTO
        {
            EventType = _event.EventType,
            Payload = _event.Payload
        };
    }
}
