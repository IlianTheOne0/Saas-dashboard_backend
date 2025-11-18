using Kafka.Interfaces;

namespace Kafka.Entities;

using Confluent.Kafka;

public class Producer : IProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic = null!;

    public Producer(ProducerConfig config, string topic)
    {
        if (config == null) { throw new ArgumentNullException("'config' argument can not be null!"); }
        if (topic == null) { throw new ArgumentNullException("'topic' argument can not be null!"); }

        _producer = new ProducerBuilder<Null, string>(config).Build();
        _topic = topic;
    }

    public async void SendMessageAsync(string message)
    {
        try
        {
            var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
            Console.WriteLine($"Message '{message}' delivered to '{result.TopicPartitionOffset}'");
        }
        catch (ProduceException<Null, string> e) { Console.WriteLine($"Delivery failed: {e.Error.Reason}"); }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}