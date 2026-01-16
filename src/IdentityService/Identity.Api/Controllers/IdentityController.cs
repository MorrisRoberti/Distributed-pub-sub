using Identity.Shared;
using Identity.Business.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
namespace Identity.Api.Controllers;

[ApiController]
[Route("/api/auth")]
public class IdentityController : ControllerBase
{
    private readonly IBusiness _business;

    private readonly ILogger<IdentityController> _logger;

    public IdentityController(IBusiness business, ILogger<IdentityController> logger)
    {
        _business = business;
        _logger = logger;
    }

    [HttpPost("auth", Name = "AuthorizeAsync")]
    public async Task<ActionResult> AuthorizeAsync(UserCredentialsDTO userCredentials)
    {
        _logger.LogInformation($"HTTP POST: Received request to authorize user with id {userCredentials.UserId}");

        // i get the credentials and check if the UserId is already in the db
        var (credentials, message) = await _business.AuthorizeUserAsync(userCredentials);

        // controls and logs
        if (credentials == null)
        {
            _logger.LogInformation($"HTTP POST: Error in authorization of user with id {userCredentials.UserId} -> {message}");
            return message switch
            {
                "Wrong Token" => Unauthorized(new { error = message }),
                "Expired Token" => StatusCode(StatusCodes.Status403Forbidden, new { error = message }),
                "Requests finished" => Unauthorized(new { error = message })
            };
        }

        _logger.LogInformation($"HTTP POST: Successfully authorized user with id {userCredentials.UserId}");

        return Ok(credentials);
    }
}