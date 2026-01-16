using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;
using Identity.Repository.Abstractions;
using Identity.Repository.Models;
using Identity.Shared;
namespace Identity.Repository;

public class Repository(IdentityDbContext identityDbContext) : IRepository
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await identityDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await identityDbContext.Database.BeginTransactionAsync();
    }

    public async Task<User> GetUserFromIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await identityDbContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<User> CreateUserAsync(Guid userId, string apiToken, CancellationToken cancellationToken = default)
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
