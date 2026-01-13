using Registry.Shared;
namespace Registry.Business.Abstractions;

public interface IBusiness
{
    Task<string> CreateSubscriptionAsync(SubscriptionDTO subscription, CancellationToken cancellationToken = default);

    Task<SubscriptionDTO?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);

}