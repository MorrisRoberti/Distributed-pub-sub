using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EventEngine.Shared;
using EventEngine.Repository.Abstractions;
using System.Text.Json;

namespace EventEngine.Business.Kafka;

public class SubscriptionConsumerWorker : BackgroundService
{
    private readonly ILogger<SubscriptionConsumerWorker> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IConsumer<string, string> consumer;
    private readonly IConfiguration configuration;
    private readonly ConsumerConfig config;

    public SubscriptionConsumerWorker(ILogger<SubscriptionConsumerWorker> _logger, IServiceProvider _serviceProvider, IConfiguration _configuration)
    {
        logger = _logger;
        serviceProvider = _serviceProvider;
        configuration = _configuration;
        // I tell the consumer where kafka is listening on
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "event-engine-group", // Defining the consumer group
            AutoOffsetReset = AutoOffsetReset.Earliest, // Recovers all the messages of the topic, the old ones to
            EnableAutoCommit = true
        };
        consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // i subscribe my consumer to the corresponding kafka topic
        consumer.Subscribe("subscription-updates-topic");

        logger.LogInformation("EventEngine Consumer listening...");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // i consume the record in the topic queue
                    var result = consumer.Consume(cancellationToken);

                    if (result is not null)
                    {
                        try
                        {

                            // deserialize the data and update the db through ProcessMessage
                            SubscriptionDTO? subscription = JsonSerializer.Deserialize<SubscriptionDTO>(result.Message.Value);


                            if (subscription is not null)
                            {

                                logger.LogInformation($"Received subscription: {subscription.Id}");

                                await ProcessMessage(subscription);
                            }
                        }
                        catch (JsonException ex)
                        {
                            logger.LogError($"The deserialization of consumer message {result.Message.Value} failed");
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    logger.LogError($"Errore consuming: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessage(SubscriptionDTO subscription)
    {
        if (subscription is null) return;

        using (var scope = serviceProvider.CreateScope())
        {
            // i get the repo and update the Subscription table with the updated subscription
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
            await repository.UpsertSubscriptionAsync(subscription);
            logger.LogInformation($"Synchronization of local db with subscription {subscription.Id} completed.");
        }
    }
}