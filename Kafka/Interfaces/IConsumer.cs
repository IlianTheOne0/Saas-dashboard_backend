namespace Kafka.Interfaces;

interface IConsumer
{
    Task Start(CancellationToken cancellationToken);
    void Dispose();
}