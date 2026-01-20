using EventEngine.ClientHttp.Abstractions;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Http;
namespace EventEngine.ClientHttp;

// IHttpClientFactory is the .NET service that handles the HttpClient lifecycle
// with the Factory it's possible to create different types of HttpClients
public class ClientHttp(IHttpClientFactory httpClientFactory) : IClientHttp
{

    public async Task<(bool IsSuccess, int? StatusCode, string? Error)> SendNotificationAsync(string url, string payload, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ClientHttp");
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            // http call to url, with content 
            var response = await client.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, (int)response.StatusCode, msg);
            }
            return (response.IsSuccessStatusCode, (int)response.StatusCode, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, (int?)ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }
}