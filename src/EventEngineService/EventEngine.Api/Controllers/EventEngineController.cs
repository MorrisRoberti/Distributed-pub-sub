using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EventEngine.Shared;
using EventEngine.Business.Abstractions;
namespace EventEngine.Api.Controllers;

// The [Route] attribute tells the program that this is a controller, so the routing middleware will make the HTTP request available in here
[ApiController]
[Route("api/event")]
public class EventController(IBusiness business, ILogger<EventController> logger) : ControllerBase
{

    // The POST endpoint to publish an event. This checks the dto and calls the business layer to create the event
    [HttpPost("publish", Name = "CreateEvent")]
    public async Task<ActionResult<Guid>> CreateEvent(EventDTO? _event) // i need to put "_event" because "event" is a keyword
    {

        if (_event is null)
        {
            logger.LogWarning($"HTTP POST: Event was null");
            return BadRequest();
        }

        logger.LogInformation($"HTTP POST: Received request to publish an Event");

        Guid evId = await business.CreateEventAsync(_event);

        logger.LogInformation($"HTTP POST: Successfully published Event {evId}");

        return CreatedAtAction(nameof(GetEvent), new { eventId = evId }, evId);
    }


    // The GET endpoint to obtain information about the event from its id. It uses eventId to search the event in the database
    [HttpGet("{eventId:guid}", Name = "GetEvent")]
    public async Task<ActionResult<EventDTO?>> GetEvent(Guid eventId)
    {
        logger.LogInformation($"HTTP GET: Received request to read Event with Id {eventId}");

        EventDTO? _event = await business.GetEventAsync(eventId);

        if (_event is null)
        {
            logger.LogWarning($"HTTP GET: Event {eventId} not found");
            return NotFound(new { Message = $"Event {eventId} not found" });
        }

        logger.LogInformation($"HTTP GET: Successfully read Event {eventId}");

        return Ok(_event);
    }
}