using Registry.Repository.Abstractions;
using Registry.Repository.Models;
namespace Registry.Repository;

public class Repository(SubscriptionDbContext subscriptionDbContext) : IRepository
{
    public async Task CreateSubscriptionAsync(string UserId, string EventType, string CallbackUrl, CancellationToken cancellationToken = default)
    {

        // insert the subscription in db
        // i need to chose which fields I need to insert the record in db
        Subscription sub = new Subscription();
        sub.UserId = UserId;
        sub.EventType = EventType;
        sub.CallbackUrl = CallbackUrl;
        sub.IsActive = true;

        await subscriptionDbContext.Subscriptions.AddAsync(sub, cancellationToken);
    }

    public async Task<Subscription?> GetSubscriptionAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        // return await subscriptionDbContext.Subscriptions.Where(s => s.Id == subscriptionId).FirstOrDefaultAsync(cancellationToken);

        return new Subscription();
    }
}
