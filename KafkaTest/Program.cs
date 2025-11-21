using KafkaTest.Services;

using Kafka.Config;
using Kafka.Entities;

namespace KafkaTest;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Text.Json;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("The test has started!");

        using IHost host = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) => { _setupServices(services); }) .Build();

        Console.WriteLine("Retrieving Kafka Producer...");
        var kafkaService = host.Services.GetRequiredService<Kafka.Entities.Kafka>();
        
        Console.WriteLine("Starting host...");
        await host.StartAsync();

        await host.WaitForShutdownAsync();

        Console.WriteLine("The test has finished!");
    }

    private static void _setupServices(IServiceCollection services)
    {
        Console.WriteLine("Configuring services...!");

        services.AddSingleton<KafkaProducer>(_ => new KafkaProducer(KafkaConfig.GetProducerConfig(), KafkaConfig.GetProducerTopic()));
        services.AddSingleton<KafkaConsumer>
        (
            provider =>
            {
                var kafkaProducer = provider.GetRequiredService<Kafka.Entities.Kafka>();
                return new KafkaConsumer(KafkaConfig.GetConsumerConfig(), KafkaConfig.GetConsumerTopic(), message => _handleMessageReceiving(message, kafkaProducer));
            }
        );
        services.AddSingleton<Kafka.Entities.Kafka>(provider =>
        {
            var producer = provider.GetRequiredService<KafkaProducer>();
            return new Kafka.Entities.Kafka(producer);
        });

        services.AddHostedService<KafkaConsumerService>();
    }

    private static async Task _handleMessageReceiving(string message, Kafka.Entities.Kafka producer)
    {
        Console.WriteLine("------------------------------------------------");
        Console.WriteLine($"[RECEIVED] Message from Frontend: {message}");

        try
        {
            var replayData = new { status = "Processed", originalMessage = "Answer" + message, processedAt = DateTime.Now };
            string replyMessage = JsonSerializer.Serialize(replayData);

            await producer.SendMessage(replyMessage);

            Console.WriteLine($"[SENT] Reply to Frontend: {replyMessage}");
        }
        catch (Exception error) { Console.WriteLine($"[ERROR] Failed to send reply: {error.Message}"); }
        Console.WriteLine("------------------------------------------------");
    }
}