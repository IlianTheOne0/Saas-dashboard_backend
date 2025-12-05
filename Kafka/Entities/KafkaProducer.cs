using Kafka.Interfaces;

namespace Kafka.Entities;

using Utils;

using Confluent.Kafka;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private const string TAG = "KAFKA-PRODUCER";
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;

    public KafkaProducer(ProducerConfig config, string topic)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentNullException(nameof(topic));

        _topic = topic;

        Logger.Debug(TAG, $"Creating producer for topic: {_topic}");
        _producer = new ProducerBuilder<Null, string>(config).SetKeySerializer(Serializers.Null).SetValueSerializer(Serializers.Utf8).Build();
    }

    public async Task SendMessage(string message)
    {
        try
        {
            Logger.Debug(TAG, $"Sending message. Payload Size: {message.Length}");

            var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });

            if (result.Status == PersistenceStatus.Persisted) { Logger.Info(TAG, $"Success! Delivered to '{result.TopicPartitionOffset}'"); }
            else { Logger.Warn(TAG, $"Message sent but status is: {result.Status}"); }
        }
        catch (ProduceException<Null, string> error) { Logger.Error(TAG, $"Delivery failed: {error.Error.Reason}"); }
        catch (Exception error) { Logger.Error(TAG, "General Producer Error", error); }
    }

    public void Dispose()
    {
        try { Logger.Debug(TAG, "Flushing producer queue (10s timeout)..."); _producer?.Flush(TimeSpan.FromSeconds(10)); Logger.Debug(TAG, "Flush complete."); }
        catch (Exception error) { Logger.Error(TAG, "Error during flush", error); }

        _producer?.Dispose();
    }
}