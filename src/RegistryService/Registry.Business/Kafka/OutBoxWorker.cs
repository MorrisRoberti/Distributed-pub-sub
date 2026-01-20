using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Registry.Repository.Abstractions;
using Registry.Shared;
using System.Text.Json;

namespace Registry.Business.Kafka;

// This is a BackgroundService so it is created and ExecuteAsync is launched and stays in the background
// This worker implements the Transacitonal Outbox Pattern: it takes the OutboxMessages from the corresponding table
// and if there are any it sends them to kafka
public class OutboxWorker(IServiceProvider serviceProvider, ProducerServiceWithSubscription producer) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                // Obtaining the service IRepository from the services
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

                // Obtaining the pending messages (the ones with ProcessedOnUtc is null )
                var pendingMessages = await repository.GetPendingOutboxMessagesAsync();

                // Trying to send all the pending messages in the queue
                foreach (var msg in pendingMessages)
                {

                    // Obtaining the dto from the payload of the pending OutboxMessage msg
                    SubscriptionDTO? sub = JsonSerializer.Deserialize<SubscriptionDTO>(msg.Payload);

                    if (sub is not null)
                    {
                        // If the payload (deserialized) is ok try to resend the message to kakfa
                        bool isSent = await producer.PublishSubscriptionAsync(sub);

                        if (isSent)
                        {
                            // If sent correctly mark the message as sent by giving value to ProcessedOnUtc
                            await repository.MarkOutboxMessageAsProcessedAsync(msg.Id);
                        }
                    }
                }
            }
            // This is useful because without it the worker would continue to query the db looking for pending messages and
            // that would be a waste of resources            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}