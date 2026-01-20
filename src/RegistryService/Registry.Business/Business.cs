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

        using var transaction = await repository.BeginTransactionAsync(cancellationToken);
        try
        {

            Subscription subCreated = await repository.CreateSubscriptionAsync(subscription.UserId, subscription.EventType, subscription.CallbackUrl, cancellationToken);

            // Once the subscription is created I create an entry in the OutboxMessages table, for kafka sync
            await repository.AddOutboxMessageAsync(subCreated, "Created", cancellationToken);

            await repository.SaveChangesAsync(cancellationToken);

            // if no errors occur I should reach this point with objects correctly create in the db, so I can commit
            await transaction.CommitAsync();

            return subCreated.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating subscription on db");
            // Actually this is a bit useless because with "using" EF Core automatically rollsback the transaction if there is an error
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SubscriptionDTO?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {

        var sub = await repository.GetSubscriptionAsync(subscriptionId, cancellationToken);

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
        // Get the Subcription from db
        var sub = await repository.GetSubscriptionAsync(subscriptionId, cancellationToken);

        if (sub is null)
            return null;

        using var transaction = await repository.BeginTransactionAsync(cancellationToken);
        try
        {

            // Update the fields
            sub.UserId = subscription.UserId;
            sub.EventType = subscription.EventType;
            sub.CallbackUrl = subscription.CallbackUrl;
            sub.IsActive = subscription.IsActive;

            // Synchronously update the subscription
            // NOTE: since I'm just modifying the state of the object in the subscriptionDbContext I don't need to do it asynchronously
            repository.UpdateSubscription(sub);

            // Create a new record in OutboxMessages to signal that a record has been updated
            await repository.AddOutboxMessageAsync(sub, "Updated", cancellationToken);

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
            // Actually this is a bit useless because with "using" EF Core automatically rollsback the transaction if there is an error
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var sub = await repository.GetSubscriptionAsync(subscriptionId, cancellationToken);

        if (sub is null)
            return false;

        using var transaction = await repository.BeginTransactionAsync(cancellationToken);
        try
        {

            sub.DeletedAt = DateTime.UtcNow;
            sub.IsActive = false;

            // I use the update because i'm doing a soft delete
            repository.UpdateSubscription(sub);

            await repository.AddOutboxMessageAsync(sub, "Deleted", cancellationToken);

            await repository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting subscription on db");
            // Actually this is a bit useless because with "using" EF Core automatically rollsback the transaction if there is an error
            await transaction.RollbackAsync();
            throw;
        }
    }
}
