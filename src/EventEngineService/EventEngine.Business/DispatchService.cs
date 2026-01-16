using EventEngine.Repository.Models;
using EventEngine.Repository.Abstractions;
using EventEngine.ClientHttp.Abstractions;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
namespace EventEngine.Business;

public class DispatchService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DispatchService> _logger;

    public DispatchService(
        IServiceProvider serviceProvider,
        ILogger<DispatchService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DispatchService launched...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
                var clientHttp = scope.ServiceProvider.GetRequiredService<IClientHttp>();

                // i create a PENDING DispatchLog record for each Subscription to the Events
                await CreateDispatchLogsAsync(repository, cancellationToken);

                // i dispatch all the PENDING DispatchLogs, both the newly created and the ququed ones
                await ProcessPendingLogsAsync(repository, clientHttp, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore during the dispatch cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private async Task CreateDispatchLogsAsync(IRepository repository, CancellationToken cancellationToken)
    {
        // i get the non-processed events
        var newEvents = await repository.GetUnprocessedEventsAsync(cancellationToken);

        foreach (var ev in newEvents)
        {
            // i query the Subscriptions table to find (active non-deleted) subscriptions interested in the current event ev
            var subscriptions = await repository.GetSubscriptionsFromEventTypeAsync(ev.EventType, cancellationToken);
            foreach (var sub in subscriptions)
            {
                // for each subscription interested in the event I add a new DisptachLog with the status PENDING
                await repository.CreateDispatchLogAsync(ev.Id, sub.Id, cancellationToken);
            }

            // the event is processed in the sense that i've created the necessary Dispatched logs
            ev.Processed = true;
        }

        if (newEvents.Any())
            await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessPendingLogsAsync(IRepository repository, IClientHttp clientHttp, CancellationToken cancellationToken)
    {
        // i get all the DispatchLogs with PENDING status and Attempts < 3
        var pendingLogs = await repository.GetPendingDispatchLogsAsync(cancellationToken);

        if (!pendingLogs.Any()) return;

        foreach (var log in pendingLogs)
        {
            // i get the CallbackUrl from the SubscriptionId in the DispatchLog
            var callbackUrl = await repository.GetCallbackUrlOfSubscription(log.SubscriptionId, cancellationToken);

            if (string.IsNullOrEmpty(callbackUrl))
            {
                log.Status = "FAILED";
                log.ErrorMessage = "CallbackUrl not found";
                continue;
            }

            try
            {
                log.Attempts++;
                log.DispatchedAt = DateTime.UtcNow;

                // http call to the url with the payload as content
                var response = await clientHttp.SendNotificationAsync(callbackUrl, log.Event!.Payload, cancellationToken);

                if (response.IsSuccess)
                {
                    log.Status = "SUCCESS";
                    log.ErrorMessage = null;
                }
                else
                {
                    log.Status = "FAILED";
                    log.ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.Error}";
                }
            }
            catch (Exception ex)
            {
                log.Status = "FAILED";
                log.ErrorMessage = ex.Message;
                _logger.LogWarning($"Failed sending for DispatchLog {log.Id}");
            }
        }

        await repository.SaveChangesAsync(cancellationToken);
    }
}