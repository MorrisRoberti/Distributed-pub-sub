using Registry.Repository.Models;
namespace Registry.Repository.Abstractions;

public interface IRepository
{

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Subscription> CreateSubscriptionAsync(string UserId, string EventType, string CallbackUrl, CancellationToken cancellationToken = default);

    Task<Subscription?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    void UpdateSubscription(Subscription subscription);
    void DeleteSubscription(Subscription subscription);
}