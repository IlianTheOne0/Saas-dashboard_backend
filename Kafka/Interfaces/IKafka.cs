namespace Kafka.Interfaces;

interface IKafka
{
    Task SendMessage(string message);
}