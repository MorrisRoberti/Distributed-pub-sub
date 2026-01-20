using Identity.Shared;
using Identity.Business.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
namespace Identity.Api.Controllers;

// The [Route] attribute tells the program that this is a controller, so the routing middleware will make the HTTP request available in here
[ApiController]
[Route("/api/auth")]
public class IdentityController(IBusiness business, ILogger<IdentityController> logger) : ControllerBase
{

    // This is the endpoint to authorize the users, it has a POST action and it accepts a json with UserId and ApiToken
    // Basically checks the db to see if the user is present, if not it creates a token and returns it, if it is, just 
    // checks if the token of the request matches the one in the db, if yes the user gets authorized, else returns an error
    [HttpPost(Name = "AuthorizeAsync")]
    public async Task<ActionResult> AuthorizeAsync(UserCredentialsDTO userCredentials)
    {
        logger.LogInformation($"HTTP POST: Received request to authorize user with id {userCredentials.UserId}");

        // i get the credentials and check if the UserId is already in the db
        // I use the credentials field to check the result of auth, and the message field to see what was the cause of error
        var (credentials, errorMessage) = await business.AuthorizeUserAsync(userCredentials);

        // controls and logs
        if (credentials == null)
        {
            logger.LogInformation($"HTTP POST: Error in authorization of user with id {userCredentials.UserId} -> {errorMessage}");
            return errorMessage switch
            {
                "Wrong Token" => Unauthorized(new { error = errorMessage }),
                "Expired Token" => StatusCode(StatusCodes.Status403Forbidden, new { error = errorMessage }),
                "Requests finished" => Unauthorized(new { error = errorMessage })
            };
        }

        logger.LogInformation($"HTTP POST: Successfully authorized user with id {userCredentials.UserId}");

        return Ok(credentials);
    }
}