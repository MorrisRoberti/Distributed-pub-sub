using Registry.Shared;
using Registry.Repository.Abstractions;
using Registry.Repository.Models;
using Registry.Business.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace Registry.Business;

public class Business(IRepository repository, ILogger<Business> logger) : IBusiness
{

    public async Task<Guid> CreateSubscriptionAsync(SubscriptionDTO subscription, CancellationToken cancellationToken = default)
    {

        using var transaction = await repository.BeginTransactionAsync();
        try
        {

            Subscription subCreated = await repository.CreateSubscriptionAsync(subscription.UserId, subscription.EventType, subscription.CallbackUrl, cancellationToken);

            await repository.AddOutboxMessageAsync(subCreated);

            await repository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync();

            return subCreated.Id;
        }
        catch (Exception ex)
        {
            logger.LogError("Error while creating subscription on db");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SubscriptionDTO?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
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

    public async Task<SubscriptionDTO?> UpdateSubscriptionAsync(Guid subscriptionId, SubscriptionDTO subscription, CancellationToken cancellationToken = default)
    {
        var sub = await repository.GetSubscriptionAsync(subscriptionId);

        if (sub is null)
            return null;

        sub.UserId = subscription.UserId;
        sub.EventType = subscription.EventType;
        sub.CallbackUrl = subscription.CallbackUrl;
        sub.IsActive = subscription.IsActive;

        repository.UpdateSubscription(sub);

        await repository.AddOutboxMessageAsync(sub);

        await repository.SaveChangesAsync(cancellationToken);

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

    public async Task<bool> DeleteSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var sub = await repository.GetSubscriptionAsync(subscriptionId);

        if (sub is null)
            return false;


        repository.DeleteSubscription(sub);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
