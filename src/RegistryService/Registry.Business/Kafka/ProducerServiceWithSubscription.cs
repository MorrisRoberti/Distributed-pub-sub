using Confluent.Kafka;
using Registry.Shared;
using System.Text.Json;
using Microsoft.Extensions.Logging;
namespace Registry.Business.Kafka;

public class ProducerServiceWithSubscription
{
    private readonly ProducerConfig _config;
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<ProducerServiceWithSubscription> _logger;

    public ProducerServiceWithSubscription(ILogger<ProducerServiceWithSubscription> logger)
    {
        _logger = logger;
        _config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<string, string>(_config).Build();
    }

    public async Task<bool> PublishSubscriptionAsync(SubscriptionDTO subscription)
    {

        try
        {
            var messageValue = JsonSerializer.Serialize(subscription);


            var result = await _producer.ProduceAsync(KafkaTopics.SubscriptionUpdates, new Message<string, string>
            {
                Key = subscription.Id.ToString(),
                Value = messageValue
            });

            _logger.LogInformation($"Message sent to Kafka: {result.Status}");
            return result.Status == PersistenceStatus.Persisted;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending the message to Kafka: {ex.Message}");
            return false;
        }
    }
}