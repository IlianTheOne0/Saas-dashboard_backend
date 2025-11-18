namespace Kafka.Interfaces;

interface IProducer
{
    void SendMessageAsync(string message);
    void Dispose();
}