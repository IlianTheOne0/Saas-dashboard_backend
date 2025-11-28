using Kafka.Interfaces;

namespace Kafka.Entities;

using Confluent.Kafka;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;
    public KafkaProducer(ProducerConfig config, string topic)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentNullException(nameof(topic));

        _topic = topic;

        _producer = new ProducerBuilder<Null, string>(config).SetKeySerializer(Serializers.Null).SetValueSerializer(Serializers.Utf8).Build();
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            Console.WriteLine($"[Producer] Sending: {message}");

            var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });

            Console.WriteLine($"[Producer] Message '{message}' delivered to '{result.TopicPartitionOffset}'");
        }
        catch (ProduceException<Null, string> e) { Console.WriteLine($"[Producer] Kafka Delivery failed: {e.Error.Reason}"); }
        catch (Exception e) { Console.WriteLine($"[Producer] General Error: {e.Message}"); }
    }

    public void Dispose()
    {
        try { _producer?.Flush(TimeSpan.FromSeconds(10)); }
        catch (Exception ex) { Console.WriteLine($"[Producer] Error during flush: {ex.Message}"); }

        _producer?.Dispose();
    }
}