using Registry.Repository.Models;
namespace Registry.Repository.Abstractions;

public interface IRepository
{

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task CreateSubscriptionAsync(string UserId, string EventType, string CallbackUrl, CancellationToken cancellationToken = default);

    Task<Subscription?> GetSubscriptionAsync(int subscriptionId, CancellationToken cancellationToken = default);
}