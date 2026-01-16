using System.Net.Http.Json;
using Identity.Shared;
using Microsoft.Extensions.Logging;

namespace Identity.ClientHttp;

public class IdentityClientHttp
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityClientHttp> _logger;

    public IdentityClientHttp(HttpClient httpClient, ILogger<IdentityClientHttp> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<(UserCredentialsDTO? credentials, string? error)> AuthorizeAsync(UserCredentialsDTO userCredentials, CancellationToken cancellationToken = default)
    {
        try
        {
            // Chiamata POST all'endpoint del controller Identity
            var response = await _httpClient.PostAsJsonAsync("api/auth/", userCredentials, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserCredentialsDTO>(cancellationToken: cancellationToken);
                return (result, null);
            }

            // Se fallisce (401, 403, ecc.), leggiamo il messaggio d'errore
            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning($"Authorization failed: {response.StatusCode} - {errorMessage}");

            return (null, errorMessage ?? "Unauthorized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with IdentityService");
            return (null, "Service Unavailable");
        }
    }
}