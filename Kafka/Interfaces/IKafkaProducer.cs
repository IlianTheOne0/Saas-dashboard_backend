namespace Kafka.Interfaces;

interface IKafkaProducer
{
    Task SendMessage(string message);
    void Dispose();
    string GetTopic();
}