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

    public async Task<Subscription?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        Guid subIdGuid = new Guid(subscriptionId);
        return await subscriptionDbContext.Subscriptions.Where(s => s.Id == subIdGuid).FirstOrDefaultAsync(cancellationToken);
    }
}
