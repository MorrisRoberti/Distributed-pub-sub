using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;
using Identity.Repository.Abstractions;
using Identity.Repository.Models;
using Identity.Shared;
namespace Identity.Repository;

public class Repository(IdentityDbContext identityDbContext) : IRepository
{
    // Saves changes in the db
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await identityDbContext.SaveChangesAsync(cancellationToken);
    }

    // Actually I don't need it but here it is, could be useful for future implementations
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await identityDbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<User?> GetUserFromIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await identityDbContext.Users
        .FindAsync(userId, cancellationToken);
    }

    // I create the user from the userId and apiToken, with the policies of expiration being: 
    // it expires a month from now, the token can be used only for 100 requests before expiring
    public async Task<User> CreateUserAsync(string userId, string apiToken, CancellationToken cancellationToken = default)
    {
        var newUser = new User
        {
            UserId = userId,
            ApiToken = apiToken,
            TokenExpireDate = DateTime.UtcNow.AddMonths(1), // the token automatically expires at one month from its generation
            RemainingRequests = 100
        };
        await identityDbContext.Users.AddAsync(newUser, cancellationToken);
        return newUser;
    }
}
