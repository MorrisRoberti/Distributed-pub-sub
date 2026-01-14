using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EventEngine.Shared;
using EventEngine.Business.Abstractions;
namespace EventEngine.Api.Controllers;

[ApiController]
[Route("api/event")]
public class EventController : ControllerBase
{
    private readonly IBusiness _business;
    private readonly ILogger<EventController> _logger;

    public EventController(IBusiness business, ILogger<EventController> logger)
    {
        _business = business;
        _logger = logger;
    }

    [HttpPost("publish", Name = "CreateEvent")]
    public async Task<ActionResult<Guid>> CreateEvent(EventDTO? _event) // i need to put "_event" because "event" is a keyword
    {

        if (_event is null)
        {
            _logger.LogWarning($"HTTP POST: _event was null");
            return BadRequest();
        }

        _logger.LogInformation($"HTTP POST: Received request to publish an event");


        Guid evId = await _business.CreateEventAsync(_event);

        _logger.LogInformation($"HTTP POST: Successfully published event {evId}");


        return CreatedAtAction(nameof(GetEvent), new { eventId = evId }, evId);
    }


    [HttpGet("{eventId:guid}", Name = "GetEvent")]
    public async Task<ActionResult<EventDTO?>> GetEvent(Guid eventId)
    {
        _logger.LogInformation($"HTTP GET: Received request to read event with Id {eventId}");

        EventDTO? _event = await _business.GetEventAsync(eventId);

        if (_event is null)
        {
            _logger.LogWarning($"HTTP GET: Event {eventId} not found");
            return NotFound(new { Message = $"Event {eventId} not found" });
        }

        _logger.LogInformation($"HTTP GET: Successfully read event {eventId}");

        return Ok(_event);
    }
}