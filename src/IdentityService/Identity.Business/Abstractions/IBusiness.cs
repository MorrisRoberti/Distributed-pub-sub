using Identity.Shared;
namespace Identity.Business.Abstractions;

public interface IBusiness
{
    Task<(UserCredentialsDTO? credentials, string message)> AuthorizeUserAsync(UserCredentialsDTO userCredentials, CancellationToken cancellationToken = default);
    string GenerateSecureHash(string userId);
}