namespace Kafka.Interfaces;

interface IKafka
{
    void SendMessage(string message);
}