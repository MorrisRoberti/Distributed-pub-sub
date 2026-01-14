using Registry.Shared;
namespace Registry.Business.Abstractions;

public interface IBusiness
{
    Task<Guid> CreateSubscriptionAsync(SubscriptionDTO subscription, CancellationToken cancellationToken = default);
    Task<SubscriptionDTO?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<SubscriptionDTO?> UpdateSubscriptionAsync(Guid subscriptionId, SubscriptionDTO subscription, CancellationToken cancellationToken = default);
    Task<bool> DeleteSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}