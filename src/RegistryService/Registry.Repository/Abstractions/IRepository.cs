using Registry.Repository.Models;
namespace Registry.Repository.Abstractions;

public interface IRepository
{

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Subscription> CreateSubscriptionAsync(string UserId, string EventType, string CallbackUrl, CancellationToken cancellationToken = default);

    Task<Subscription?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
}