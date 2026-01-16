using Identity.Repository.Models;
using Identity.Shared;
using Microsoft.EntityFrameworkCore.Storage;

namespace Identity.Repository.Abstractions;

public interface IRepository
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<User> GetUserFromIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(string userId, string apiToken, CancellationToken cancellationToken = default);
}