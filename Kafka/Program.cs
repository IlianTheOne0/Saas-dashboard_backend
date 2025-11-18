namespace Kafka;

using KafkaConsumer;
using KafkaProducer;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(KafkaConsumer.Kafka.GetMessage());
        Console.WriteLine(KafkaProducer.Kafka.GetMessage());
    }
}