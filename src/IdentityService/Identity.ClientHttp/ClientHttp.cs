using System.Net.Http.Json;
using Identity.Shared;
using Microsoft.Extensions.Logging;

namespace Identity.ClientHttp;

// This client will be used as a dependency of RegistryService, to authorize the users who make the requests
public class IdentityClientHttp(IHttpClient httpClient, ILogger<IdentityClientHttp> logger)
{

    // Here the try-catch block is important because this client will be injected in another service, and http request are intrinsecally unsafe
    // Also the deserialization could produce an error
    public async Task<(UserCredentialsDTO? credentials, string? errorMessage)> AuthorizeAsync(UserCredentialsDTO userCredentials, CancellationToken cancellationToken = default)
    {
        try
        {
            // I make a POST to the auth endpoint, the PostAsJsonAsync method automatically serializes the userCredentials and sets the Content-Type header to application/json
            var response = await httpClient.PostAsJsonAsync("api/auth/", userCredentials, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // ReadFromJsonAsync deserializes the response.Content in a UserCredentialsDTO object
                var result = await response.Content.ReadFromJsonAsync<UserCredentialsDTO>(cancellationToken: cancellationToken);
                return (result, null);
            }

            // If the POST fails i return the message in a plain way, because i will note receive a UserCredentialsDTO object
            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning($"Authorization failed: {response.StatusCode} - {errorMessage}");

            return (null, errorMessage ?? "Unauthorized");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error communicating with IdentityService");
            return (null, "Service Unavailable");
        }
    }
}