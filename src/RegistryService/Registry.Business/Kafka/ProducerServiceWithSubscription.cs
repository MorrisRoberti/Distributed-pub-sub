using Confluent.Kafka;
using Registry.Shared;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
namespace Registry.Business.Kafka;

// I won't use the Primary Constructor because I have to configure things
public class ProducerServiceWithSubscription
{
    private readonly ProducerConfig config;
    private readonly IProducer<string, string> producer;
    private readonly ILogger<ProducerServiceWithSubscription> logger;
    private readonly IConfiguration configuration;

    public ProducerServiceWithSubscription(ILogger<ProducerServiceWithSubscription> _logger, IConfiguration _configuration)
    {
        logger = _logger;
        configuration = _configuration;
        // I tell the producer where kafka is listening on
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All, // This assures that even if the broker crashes after the sending of a message, this will still be inserted
            MessageTimeoutMs = 5000 // If there is a problem the producer retries to send the message, after 5 seconds the message is considered lost and an exception will be launched
        };
        // I build the producer with the parameters that I've established
        producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task<bool> PublishSubscriptionAsync(SubscriptionDTO subscription)
    {

        try
        {
            var messageValue = JsonSerializer.Serialize(subscription);

            // Pushes the message with the payload of the Subscription in the SubscriptionUpdates topic (defined in KafkaTopics.cs)
            var result = await producer.ProduceAsync(KafkaTopics.SubscriptionUpdates, new Message<string, string>
            {
                Key = subscription.Id.ToString(),
                Value = messageValue
            });

            logger.LogInformation($"Message sent to Kafka: {result.Status}");
            // I put the result as persisted
            return result.Status == PersistenceStatus.Persisted;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error sending the message to Kafka: {ex.Message}");
            return false;
        }
    }
}