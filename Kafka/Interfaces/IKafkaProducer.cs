namespace Kafka.Interfaces;

public interface IKafkaProducer
{
    Task SendMessage(string message);
    void Dispose();
    string GetTopic();
}