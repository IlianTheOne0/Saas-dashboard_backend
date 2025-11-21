using Kafka.Entities;

namespace KafkaTest.Services;

using Microsoft.Extensions.Hosting;

public class KafkaConsumerService : BackgroundService
{
    private readonly KafkaConsumer _consumer;

    public KafkaConsumerService(KafkaConsumer consumer) { _consumer = consumer; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) { Console.WriteLine("Kafka Consumer Service starting..."); return _consumer.Start(stoppingToken); }
}