using Registry.Repository.Abstractions;
using Registry.Repository.Models;
using Microsoft.EntityFrameworkCore;
namespace Registry.Repository;

public class Repository(SubscriptionDbContext subscriptionDbContext) : IRepository
{

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await subscriptionDbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<Subscription> CreateSubscriptionAsync(string UserId, string EventType, string CallbackUrl, CancellationToken cancellationToken = default)
    {

        Subscription sub = new Subscription();
        sub.UserId = UserId;
        sub.EventType = EventType;
        sub.CallbackUrl = CallbackUrl;
        sub.IsActive = true;

        await subscriptionDbContext.Subscriptions.AddAsync(sub, cancellationToken);
        return sub;
    }

    public async Task<Subscription?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await subscriptionDbContext.Subscriptions.Where(s => s.Id == subscriptionId).FirstOrDefaultAsync(cancellationToken);
    }
    public void UpdateSubscription(Subscription subscription)
    {
        subscriptionDbContext.Subscriptions.Update(subscription);
    }
    public void DeleteSubscription(Subscription subscription)
    {
        subscriptionDbContext.Subscriptions.Remove(subscription);
    }
}
