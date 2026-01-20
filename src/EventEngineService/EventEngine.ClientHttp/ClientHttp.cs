using EventEngine.ClientHttp.Abstractions;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Http;
namespace EventEngine.ClientHttp;

public class ClientHttp(HttpClient httpClient) : IClientHttp
{

    public async Task<(bool IsSuccess, int? StatusCode, string? Error)> SendNotificationAsync(string url, string payload, CancellationToken cancellationToken)
    {
        try
        {

            // Using the payload of the Event to insert into the request
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            // Make an HTTP call to the CallbackUrl of the current Subscription with the Event payload
            var response = await httpClient.PostAsync(url, content, cancellationToken);

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