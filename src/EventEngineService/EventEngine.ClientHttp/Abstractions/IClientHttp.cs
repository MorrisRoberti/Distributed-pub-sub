namespace EventEngine.ClientHttp.Abstractions;

public interface IClientHttp
{
    Task<(bool IsSuccess, int? StatusCode, string? Error)> SendNotificationAsync(string url, string payload, CancellationToken cancellationToken);
}