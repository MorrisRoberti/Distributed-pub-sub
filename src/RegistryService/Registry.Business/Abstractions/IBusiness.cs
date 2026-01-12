using Registry.Shared;
namespace Registry.Business.Abstractions;

public interface IBusiness
{
    Task CreateSubscriptionAsync(SubscriptionDTO subscription, CancellationToken cancellationToken = default);

    Task<SubscriptionDTO?> GetSubscriptionAsync(int subscriptionId, CancellationToken cancellationToken = default);

}