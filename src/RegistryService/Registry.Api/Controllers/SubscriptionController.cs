using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Registry.Shared;
using Registry.Business.Abstractions;
using Identity.ClientHttp;
using Identity.Shared;
namespace Registry.Api.Controllers;

// The [Route] attribute tells the program that this is a controller, so the routing middleware will make the HTTP request available in here
[ApiController]
[Route("api/subscription")]
public class SubscriptionController(IBusiness business, ILogger<SubscriptionController> logger, IdentityClientHttp identityClientHttp) : ControllerBase
{

    // This is a POST to create subscription. The first thing it actually does is call the IdentityService to authorize the User
    // NOTE: the authorization code should be put inside a middleware for all requests, in this way it would be transparent to the controller
    [HttpPost("subscribe", Name = "CreateSubscription")]
    public async Task<ActionResult> CreateSubscription(SubscriptionDTO? subscription)
    {
        if (subscription is null || subscription.UserId is null)
        {
            logger.LogWarning($"HTTP POST: subscription was invalid");
            return BadRequest();
        }

        var authRequest = new UserCredentialsDTO
        {
            UserId = subscription.UserId,
            ApiToken = subscription.ApiToken
        };

        var (authResult, error) = await identityClientHttp.AuthorizeAsync(authRequest);

        if (authResult is null)
        {
            logger.LogWarning($"HTTP POST: Auth failed for User {subscription.UserId}. Error: {error}");

            return StatusCode(StatusCodes.Status401Unauthorized, new { Message = error });
        }

        // Actually all the code until here should be put into an authorization controller
        logger.LogInformation($"HTTP POST: Received request to create a subscription for User {subscription.UserId}");

        Guid subId = await business.CreateSubscriptionAsync(subscription);

        // The returned object should be a separate DTO class
        // I'm not sure this is the right way to return a token
        var response = new
        {
            SubscriptionId = subId,
            ApiToken = authResult.ApiToken
        };

        logger.LogInformation($"HTTP POST: Successfully created subscription {subId}");

        return CreatedAtAction(nameof(GetSubscription), new { subscriptionId = subId }, response);
    }


    // This is the GET endpoint used to obtain the information of the subscription from the id
    [HttpGet("{subscriptionId:guid}", Name = "GetSubscription")]
    public async Task<ActionResult<SubscriptionDTO?>> GetSubscription(Guid subscriptionId)
    {
        logger.LogInformation($"HTTP GET: Received request to read subscription with Id {subscriptionId}");

        SubscriptionDTO? sub = await business.GetSubscriptionAsync(subscriptionId);

        if (sub is null)
        {
            logger.LogWarning($"HTTP GET: Subscription {subscriptionId} not found");
            return NotFound(new { Message = $"Subscription {subscriptionId} not found" });
        }

        logger.LogInformation($"HTTP GET: Successfully read subscription {subscriptionId}");

        return Ok(sub);
    }

    // The PUT action updates all the fields of the object
    [HttpPut("{subscriptionId:guid}", Name = "UpdateSubscription")]
    public async Task<ActionResult<SubscriptionDTO>> UpdateSubscription(Guid subscriptionId, SubscriptionDTO subscription)
    {

        logger.LogInformation($"HTTP PUT: Received request to update subscription with Id {subscriptionId}");

        SubscriptionDTO? sub = await business.UpdateSubscriptionAsync(subscriptionId, subscription);

        if (sub is null)
        {
            logger.LogWarning($"HTTP PUT: Subscription {subscriptionId} not found");
            return NotFound(new { Message = $"Subscription {subscriptionId} not found" });
        }

        logger.LogInformation($"HTTP PUT: Successfully updated subscription {subscriptionId}");


        return Ok(sub);
    }


    [HttpDelete("{subscriptionId:guid}", Name = "DeleteSubscription")]
    public async Task<ActionResult> DeleteSubscription(Guid subscriptionId)
    {

        logger.LogInformation($"HTTP DELETE: Received request to delete subscription with Id {subscriptionId}");

        bool result = await business.DeleteSubscriptionAsync(subscriptionId);

        if (!result)
        {
            logger.LogWarning($"HTTP DELETE: Subscription {subscriptionId} not found");
            return NotFound(new { Message = $"Subscription {subscriptionId} not found" });
        }

        logger.LogInformation($"HTTP DELETE: Successfully deleted subscription with Id {subscriptionId}");


        return NoContent();
    }
}
