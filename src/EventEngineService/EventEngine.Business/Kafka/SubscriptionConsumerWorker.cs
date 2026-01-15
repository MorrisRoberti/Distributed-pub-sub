using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventEngine.Shared;
using EventEngine.Repository.Abstractions;
using System.Text.Json;

namespace EventEngine.Business.Kafka;

public class SubscriptionConsumerWorker : BackgroundService
{
    private readonly ILogger<SubscriptionConsumerWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string> _consumer;

    private readonly ConsumerConfig _config;

    public SubscriptionConsumerWorker(ILogger<SubscriptionConsumerWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "event-engine-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        _consumer = new ConsumerBuilder<string, string>(_config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // i subscribe my consumer to the corresponding kafka topic
        _consumer.Subscribe("subscription-updates-topic");

        _logger.LogInformation("EventEngine Consumer listening...");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // i consume the record in the topic queue
                    var result = _consumer.Consume(cancellationToken);

                    if (result != null)
                    {
                        try
                        {

                            // deserialize the data and update the db through ProcessMessage
                            SubscriptionDTO? subscription = JsonSerializer.Deserialize<SubscriptionDTO>(result.Message.Value);


                            if (subscription != null)
                            {

                                _logger.LogInformation($"Received subscription: {subscription.Id}");

                                await ProcessMessage(subscription);
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError($"The deserialization of consumer message {result.Message.Value} failed");
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Errore consuming: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _consumer.Close();
        }
    }

    private async Task ProcessMessage(SubscriptionDTO subscription)
    {
        if (subscription == null) return;

        using (var scope = _serviceProvider.CreateScope())
        {
            // i get the repo and update the Subscription table with the updated subscription
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
            await repository.UpsertSubscriptionAsync(subscription);
            _logger.LogInformation($"Synchronization of local db with subscription {subscription.Id} completed.");
        }
    }
}