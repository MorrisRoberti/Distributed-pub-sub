using Confluent.Kafka;
using Registry.Shared;
using System.Text.Json;

namespace Registry.Business.Kafka;

public class ProducerServiceWithSubscription
{
    private readonly ProducerConfig _config;
    private readonly ILogger<ProducerServiceWithSubscription> _logger;

    public ProducerServiceWithSubscription(ILogger<ProducerServiceWithSubscription> logger)
    {
        _logger = logger;
        _config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092", // Indirizzo del broker Kafka
            Acks = Acks.All // Garantisce che Kafka confermi la ricezione
        };
    }

    public async Task<bool> PublishSubscriptionAsync(SubscriptionDTO subscription)
    {
        using var producer = new ProducerBuilder<string, string>(_config).Build();

        try
        {
            var messageValue = JsonSerializer.Serialize(subscription);

            // Usiamo l'ID come chiave per garantire l'ordine dei messaggi su Kafka
            var result = await producer.ProduceAsync(KafkaTopics.SubscriptionUpdates, new Message<string, string>
            {
                Key = subscription.Id.ToString(),
                Value = messageValue
            });

            _logger.LogInformation($"Messaggio inviato a Kafka: {result.Status}");
            return result.Status == PersistenceStatus.Persisted;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Errore invio Kafka: {ex.Message}");
            return false;
        }
    }
}