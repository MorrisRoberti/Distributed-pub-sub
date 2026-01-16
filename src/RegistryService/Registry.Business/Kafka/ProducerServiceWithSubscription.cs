using Confluent.Kafka;
using Registry.Shared;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
namespace Registry.Business.Kafka;

public class ProducerServiceWithSubscription
{
    private readonly ProducerConfig _config;
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<ProducerServiceWithSubscription> _logger;
    private readonly IConfiguration _configuration;

    public ProducerServiceWithSubscription(ILogger<ProducerServiceWithSubscription> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            MessageTimeoutMs = 5000
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