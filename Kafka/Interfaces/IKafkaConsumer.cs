namespace Kafka.Interfaces;

interface IKafkaConsumer
{
    Task Start(CancellationToken cancellationToken);
    void Dispose();
    string GetTopic();
}