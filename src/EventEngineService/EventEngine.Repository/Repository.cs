using EventEngine.Repository.Abstractions;
using EventEngine.Repository.Models;
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
}