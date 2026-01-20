using Identity.Business.Abstractions;
using Identity.Repository.Abstractions;
using Identity.Repository.Models;
using Identity.Shared;
using Microsoft.Extensions.Logging;
namespace Identity.Business;

// This is the Primary Constructor syntax, is equivalent to the normal constructor but the fields are not necessarely readonly
// so we have to pay attention to not reassign them inside the class
public class Business(IRepository repository, ILogger<Business> logger) : IBusiness
{
    // I don't need transactions here because with SaveChangesAsync, EF Core already treats this as an atomic transaction
    // NOTE: if something goes wrong with the connection to the db or stuff like that the service breaks because there is no exception handling yet
    public async Task<(UserCredentialsDTO? credentials, string errorMessage)> AuthorizeUserAsync(UserCredentialsDTO userCredentials, CancellationToken cancellationToken = default)
    {
        User? user = await repository.GetUserFromIdAsync(userCredentials.UserId, cancellationToken);

        if (user is null)
        {
            // on the first access the user record does not exist in db, so I create it
            // i create the user on db and set the token in the credentials return dto
            string token = GenerateSecureHash(userCredentials.UserId);
            await repository.CreateUserAsync(userCredentials.UserId, token, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            userCredentials.ApiToken = token;
            return (userCredentials, null);
        }

        // if the user exists I check for the token, if it is wrong or empty the user is Unauthorized
        if (string.IsNullOrEmpty(userCredentials.ApiToken) || user.ApiToken != userCredentials.ApiToken)
        {
            return (null, "Wrong Token");
        }

        // here the token in userCredentials is correct, 
        // so i check if he has other requests and the token is not expired yet
        if (user.TokenExpireDate > DateTime.UtcNow)
        {
            if (user.RemainingRequests > 0)
            {

                // I remove a request, I save the data and I return the userCredentials
                user.RemainingRequests--;
                await repository.SaveChangesAsync(cancellationToken);
                return (userCredentials, null);
            }
            else
            {
                return (null, "Requests finished");
            }
        }
        else
        {
            return (null, "Expired Token");
        }

    }

    public string GenerateSecureHash(string userId)
    {
        // i need to find an hash function to -> hash(userId, salt);
        return "token";
    }
}
