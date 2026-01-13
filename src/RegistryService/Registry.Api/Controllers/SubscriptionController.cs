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

    [HttpGet(Name = "GetSubscription/{subscriptionId}")]
    public async Task<ActionResult<SubscriptionDTO?>> GetSubscription(string subscriptionId)
    {
        SubscriptionDTO? sub = await _business.GetSubscriptionAsync(subscriptionId);
        return sub;
    }

    [HttpPut(Name = "UpdateSubscription")]
    public ActionResult<SubscriptionDTO?> UpdateSubscription(SubscriptionDTO? subscription)
    {
        return NoContent();
    }


    [HttpDelete(Name = "DeleteSubscription")]
    public ActionResult<SubscriptionDTO?> DeleteSubscription()
    {
        return NoContent();
    }
}
