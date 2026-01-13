using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Registry.Shared;
using Registry.Business.Abstractions;
namespace Registry.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{

    private readonly IBusiness _business;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(IBusiness business, ILogger<SubscriptionController> logger)
    {
        _business = business;
        _logger = logger;
    }

    [HttpPost(Name = "CreateSubscription")]
    public async Task<ActionResult<string>> CreateSubscription(SubscriptionDTO? subscription)
    {

        if (subscription is null)
        {
            _logger.LogWarning($"HTTP POST: subscriptionId was null");
            return BadRequest();
        }

        _logger.LogInformation($"HTTP POST: Received request to create a subscription for User {subscription.UserId}");

        string subId = await _business.CreateSubscriptionAsync(subscription);

        _logger.LogInformation($"HTTP POST: Successfully created subscription {subId}");

        return CreatedAtAction(nameof(GetSubscription), new { subscriptionId = subId }, subId);
    }

    [HttpGet("{subscriptionId:guid}", Name = "GetSubscription")]
    public async Task<ActionResult<SubscriptionDTO?>> GetSubscription(Guid subscriptionId)
    {
        _logger.LogInformation($"HTTP GET: Received request to read subscription with Id {subscriptionId}");

        SubscriptionDTO? sub = await _business.GetSubscriptionAsync(subscriptionId);

        if (sub is null)
        {
            _logger.LogWarning($"HTTP GET: Subscription {SubscriptionId} not found");
            return NotFound(new { Message = $"Subscription {subscriptionId} not found" });
        }

        _logger.LogInformation($"HTTP GET: Successfully read subscription {subscriptionId}");

        return Ok(sub);
    }

    [HttpPut("{subscriptionId:guid}", Name = "UpdateSubscription")]
    public async Task<ActionResult<SubscriptionDTO>> UpdateSubscription(Guid subscriptionId, SubscriptionDTO subscription)
    {

        _logger.LogInformation($"HTTP PUT: Received request to update subscription with Id {subscriptionId}");

        SubscriptionDTO? sub = await _business.UpdateSubscriptionAsync(subscriptionId, subscription);

        if (sub is null)
        {
            _logger.LogWarning($"HTTP PUT: Subscription {SubscriptionId} not found");
            return NotFound(new { Message = $"Subscription {subscriptionId} not found" });
        }

        _logger.LogInformation($"HTTP PUT: Successfully updated subscription {subscriptionId}");


        return Ok(sub);
    }


    [HttpDelete("{subscriptionId:guid}", Name = "DeleteSubscription")]
    public async Task<ActionResult> DeleteSubscription(Guid subscriptionId)
    {
        _logger.LogInformation($"HTTP DELETE: Received request to delete subscription with Id {subscriptionId}");

        bool result = await _business.DeleteSubscriptionAsync(subscriptionId);

        if (!result)
        {
            _logger.LogWarning($"HTTP DELETE: Subscription {SubscriptionId} not found");
            return NotFound(new { Message = $"Subscription {subscriptionId} not found" });
        }

        _logger.LogInformation($"HTTP DELETE: Successfully deleted subscription with Id {subscriptionId}");


        return NoContent();
    }
}
