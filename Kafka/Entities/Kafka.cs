using Kafka.Interfaces;

namespace Kafka.Entities;

public class Kafka : IKafka
{
    private readonly KafkaProducer _producer = null!;

    public Kafka(KafkaProducer producer)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    }

    public Task SendMessage(string message) => _producer.SendMessageAsync(message);
}