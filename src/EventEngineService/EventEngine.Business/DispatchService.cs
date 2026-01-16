using EventEngine.Repository.Models;
using EventEngine.Repository.Abstractions;
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

                // i create a PENDING DispatchLog record for each Subscription to the Events
                await CreateDispatchLogsAsync(repository, cancellationToken);

                // i dispatch all the PENDING DispatchLogs, both the newly created and the ququed ones
                // await ProcessPendingLogsAsync(repository, cancellationToken);
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
                await repository.CreateDispatchLogAsync(ev.Id, cancellationToken);
            }

            // the event is processed in the sense that i've created the necessary Dispatched logs
            ev.Processed = true;
        }

        if (newEvents.Any())
            await repository.SaveChangesAsync(cancellationToken);
    }

    // private async Task ProcessPendingLogsAsync(IRepository repository, CancellationToken ct)
    // {
    //     // Carichiamo i log PENDING includendo i dati dell'evento e della sottoscrizione
    //     // Nota: Assicurati di avere le Navigation Properties nel modello DispatchLog
    //     var pendingLogs = await repository.DispatchLogs
    //         .Include(l => l.Event)
    //         .Where(l => l.Status == "PENDING" && l.Attempts < 3)
    //         .ToListAsync(ct);

    //     if (!pendingLogs.Any()) return;

    //     var client = _httpClientFactory.CreateClient("WebhookClient");

    //     foreach (var log in pendingLogs)
    //     {
    //         // Recuperiamo l'URL della sottoscrizione (dovresti avere una FK verso Subscription)
    //         // Per ora ipotizziamo di recuperarla via log.Event o log.Subscription
    //         var callbackUrl = await repository.Subscriptions
    //             .Where(s => s.Id == log.SubscriptionId) // Assumendo che tu aggiunga SubscriptionId
    //             .Select(s => s.CallbackUrl)
    //             .FirstOrDefaultAsync(ct);

    //         if (string.IsNullOrEmpty(callbackUrl))
    //         {
    //             log.Status = "FAILED";
    //             log.ErrorMessage = "Callback URL non trovato o sottoscrizione rimossa.";
    //             continue;
    //         }

    //         try
    //         {
    //             log.Attempts++;
    //             log.DispatchedAt = DateTime.UtcNow;

    //             var content = new StringContent(log.Event!.Data, Encoding.UTF8, "application/json");

    //             // Chiamata HTTP con timeout breve
    //             var response = await client.PostAsync(callbackUrl, content, ct);

    //             if (response.IsSuccessStatusCode)
    //             {
    //                 log.Status = "SUCCESS";
    //                 log.ErrorMessage = null;
    //             }
    //             else
    //             {
    //                 log.Status = "FAILED";
    //                 log.ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
    //             }
    //         }
    //         catch (Exception ex)
    //         {
    //             log.Status = "FAILED";
    //             log.ErrorMessage = ex.Message;
    //             _logger.LogWarning("Invio fallito per log {LogId}: {Error}", log.Id, ex.Message);
    //         }
    //     }

    //     await repository.SaveChangesAsync(ct);
    // }
}