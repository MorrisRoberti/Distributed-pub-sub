using EventEngine.Repository.Models;
using EventEngine.Repository.Abstractions;
using EventEngine.ClientHttp.Abstractions;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
namespace EventEngine.Business;

// This is a BackgroundService that periodically checks if there are any non-processed Events and if so,
// for every Subscription it dispatches it to the corresponding CallbackUrl
public class DispatchService(IServiceProvider serviceProvider, ILogger<DispatchService> logger) : BackgroundService
{


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("DispatchService launched...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Getting the Services from the scope
                using var scope = serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
                var clientHttp = scope.ServiceProvider.GetRequiredService<IClientHttp>();

                // Creates a PENDING DispatchLog record for each Subscription to the Events
                await CreateDispatchLogsAsync(repository, cancellationToken);

                // Dispatches all the PENDING DispatchLogs, both the newly created and the ququed ones
                await ProcessPendingLogsAsync(repository, clientHttp, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore during the dispatch cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private async Task CreateDispatchLogsAsync(IRepository repository, CancellationToken cancellationToken)
    {
        // Gets the non-processed events
        var newEvents = await repository.GetUnprocessedEventsAsync(cancellationToken);

        // Creates a new DispatchLog for every Subscription submitted to that EventType
        foreach (var ev in newEvents)
        {
            // Gets the Subscriptions table to find (active non-deleted) subscriptions interested in the current event ev
            var subscriptions = await repository.GetSubscriptionsFromEventTypeAsync(ev.EventType, cancellationToken);

            foreach (var sub in subscriptions)
            {
                // For every subscription interested in the event I add a new DisptachLog with the status PENDING
                await repository.CreateDispatchLogAsync(ev.Id, sub.Id, cancellationToken);
            }

            // the event is processed in the sense that i've created the necessary Dispatched logs
            ev.Processed = true;
        }

        // If the list of events has at least one element the changes are saved in the db
        // to save the processd status (ev.Processed line)
        if (newEvents.Any())
            await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessPendingLogsAsync(IRepository repository, IClientHttp clientHttp, CancellationToken cancellationToken)
    {
        // Gets all the DispatchLogs with PENDING status and Attempts < 3
        var pendingLogs = await repository.GetPendingDispatchLogsAsync(cancellationToken);

        if (!pendingLogs.Any()) return;

        foreach (var dispatchedLog in pendingLogs)
        {
            // Gets the CallbackUrl from the SubscriptionId in the DispatchLog
            var callbackUrl = await repository.GetCallbackUrlOfSubscription(dispatchedLog.SubscriptionId, cancellationToken);

            // It should not be possible for the CallbackUrl to be null because it is required in the SubscriptionModel 
            if (string.IsNullOrEmpty(callbackUrl))
            {
                dispatchedLog.Status = "FAILED";
                dispatchedLog.ErrorMessage = "CallbackUrl not found";
                // Go to the next pending logs
                continue;
            }

            try
            {
                // Updats the number of attempts
                dispatchedLog.Attempts++;
                // If the SendNotificationAsync fails we still save the time in the DispatchedAt field, in this way 
                // we can know whenever the send is occurred, failed or not
                dispatchedLog.DispatchedAt = DateTime.UtcNow;

                // http call to the url with the payload as content
                var (IsSuccess, StatusCode, Error) = await clientHttp.SendNotificationAsync(callbackUrl, dispatchedLog.Event!.Payload, cancellationToken);

                if (IsSuccess)
                {
                    dispatchedLog.Status = "SUCCESS";
                    dispatchedLog.ErrorMessage = null;
                }
                else
                {
                    dispatchedLog.Status = "FAILED";
                    dispatchedLog.ErrorMessage = $"HTTP {StatusCode}: {Error}";
                }

            }
            catch (Exception ex)
            {
                dispatchedLog.Status = "FAILED";
                dispatchedLog.ErrorMessage = ex.Message;
                logger.LogWarning($"Failed sending for DispatchLog {dispatchedLog.Id}");
            }
        }

        // After dispatching every Event, the DispatchLogs are updated
        await repository.SaveChangesAsync(cancellationToken);
    }
}