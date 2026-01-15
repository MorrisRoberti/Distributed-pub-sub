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

            await repository.AddOutboxMessageAsync(subCreated, "Created");

            await repository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync();

            return subCreated.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating subscription on db");
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

        using var transaction = await repository.BeginTransactionAsync();
        try
        {

            sub.UserId = subscription.UserId;
            sub.EventType = subscription.EventType;
            sub.CallbackUrl = subscription.CallbackUrl;
            sub.IsActive = subscription.IsActive;

            repository.UpdateSubscription(sub);

            await repository.AddOutboxMessageAsync(sub, "Updated");

            await repository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync();

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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while Updating subscription on db");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var sub = await repository.GetSubscriptionAsync(subscriptionId);

        if (sub is null)
            return false;

        using var transaction = await repository.BeginTransactionAsync();
        try
        {

            sub.DeletedAt = DateTime.UtcNow;
            sub.IsActive = false;

            // I use the update because i'm doing a soft delete
            repository.UpdateSubscription(sub);

            await repository.AddOutboxMessageAsync(sub, "Deleted");

            await repository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting subscription on db");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
