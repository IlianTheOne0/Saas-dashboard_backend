namespace Kafka.Interfaces;

interface IKafkaProducer
{
    Task SendMessageAsync(string message);
    void Dispose();
    string GetTopic();
}