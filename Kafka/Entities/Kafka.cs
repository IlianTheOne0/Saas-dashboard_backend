using Kafka.Interfaces;

namespace Kafka.Entities;

public class Kafka : IKafka
{
    private readonly Producer _producer = null!;

    public Kafka(Producer producer)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    }

    public void SendMessage(string message) => _producer.SendMessageAsync(message);
}