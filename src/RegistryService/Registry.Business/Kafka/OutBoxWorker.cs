using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Registry.Repository.Abstractions;
using Registry.Shared;
using System.Text.Json;

namespace Registry.Business.Kafka;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ProducerServiceWithSubscription _producer;

    public OutboxWorker(IServiceProvider serviceProvider, ProducerServiceWithSubscription producer)
    {
        _serviceProvider = serviceProvider;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

                // i get the pending messages (the ones with ProcessedOnUtc == null )
                var pendingMessages = await repository.GetPendingOutboxMessagesAsync();

                foreach (var msg in pendingMessages)
                {

                    // i get the dto from the payload of the pending OutboxMessage msg
                    SubscriptionDTO? sub = JsonSerializer.Deserialize<SubscriptionDTO>(msg.Payload);

                    if (sub != null)
                    {
                        // i retry to send it to kafka
                        bool isSent = await _producer.PublishSubscriptionAsync(sub);

                        if (isSent)
                        {
                            // if sent correctly i mark the message as sent by giving value to ProcessedOnUtc
                            await repository.MarkOutboxMessageAsProcessedAsync(msg.Id);
                        }
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}