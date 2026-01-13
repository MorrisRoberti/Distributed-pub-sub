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
        string subId = await _business.CreateSubscriptionAsync(subscription);

        return CreatedAtAction(nameof(GetSubscription), new { subscriptionId = subId }, subId);
    }

    [HttpGet("{subscriptionId:guid}", Name = "GetSubscription")]
    public async Task<ActionResult<SubscriptionDTO?>> GetSubscription(Guid subscriptionId)
    {
        SubscriptionDTO? sub = await _business.GetSubscriptionAsync(subscriptionId);
        return sub;
    }

    [HttpPut("{subscriptionId:guid}", Name = "UpdateSubscription")]
    public async Task<ActionResult<SubscriptionDTO>> UpdateSubscription(Guid subscriptionId, SubscriptionDTO subscription)
    {
        SubscriptionDTO? sub = await _business.UpdateSubscriptionAsync(subscriptionId, subscription);

        if (sub is null)
        {
            return NotFound(new { Message = $"Subscription {subscriptionId} not found" });
        }


        return Ok(sub);
    }


    [HttpDelete("{subscriptionId:guid}", Name = "DeleteSubscription")]
    public async Task<ActionResult> DeleteSubscription(Guid subscriptionId)
    {
        bool result = await _business.DeleteSubscriptionAsync(subscriptionId);

        if (!result)
            return NotFound(new { Message = $"Subscription {subscriptionId} not found" });

        return NoContent();
    }
}
