using Registry.Shared;
using Registry.Repository.Abstractions;
using Registry.Business.Abstractions;
using Microsoft.Extensions.Logging;
namespace Registry.Business;

public class Business(IRepository repository, ILogger<Business> logger) : IBusiness
{

    public async Task CreateSubscriptionAsync(SubscriptionDTO subscription, CancellationToken cancellationToken = default)
    {
        await repository.CreateSubscriptionAsync(subscription.UserId, subscription.EventType, subscription.CallbackUrl, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<SubscriptionDTO?> GetSubscriptionAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {

        var sub = await repository.GetSubscriptionAsync(subscriptionId);

        if (sub is null)
            return null;

        return new SubscriptionDTO
        {
            Id = sub.Id,
            UserId = sub.UserId,
            EventType = sub.EventType,
            CallbackUrl = sub.CallbackUrl,
            IsActive = sub.IsActive,
            CreatedAt = sub.CreatedAt
        };
    }
}
