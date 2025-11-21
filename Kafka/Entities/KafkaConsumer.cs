using Kafka.Interfaces;

namespace Kafka.Entities;

using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

public class KafkaConsumer : IKafkaConsumer, IDisposable
{
    private IConsumer<Ignore, string>? _consumer;

    private ConsumerConfig _config = null!;
    private string _topic = null!;

    private readonly Func<string, Task> _messageHandler;

    public KafkaConsumer(ConsumerConfig config, string topic, Func<string, Task> messageHandler)
    {
        if (config == null) { throw new ArgumentNullException("'config' argument can not be null!"); }
        if (topic == null) { throw new ArgumentNullException("'topic' argument can not be null!"); }
        if (messageHandler == null) { throw new ArgumentNullException("'messageHandler' argument can not be null!"); }

        _config = config;
        _topic = topic;
        _messageHandler = messageHandler;

        _consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
    }

    public Task Start(CancellationToken cancellationToken) =>
    Task.Run
    (
        async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await CreateTopicIfNotExists();

                    _consumer!.Subscribe(_topic);
                    Console.WriteLine($"Consumer subscribed to: {_topic}");

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = _consumer!.Consume(cancellationToken);

                            if (result != null)
                            {
                                Console.WriteLine($"[KAFKA RAW RECEIVED] Key: {result.Message.Key}, Value: {result.Message.Value}");
                                await _messageHandler(result.Message.Value);
                            }
                        }
                        catch (ConsumeException error)
                        {
                            Console.WriteLine($"Error consuming message: {error.Error.Reason}. Retrying...");
                            await Task.Delay(1000, cancellationToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Consumer operation canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FATAL ERROR in Consumer Loop: {ex.Message}");
                    Console.WriteLine("Retrying connection in 5 seconds...");
                    await Task.Delay(5000, cancellationToken);
                }
            }
            _consumer!.Close();
        },
        cancellationToken
    );

    private async Task CreateTopicIfNotExists()
    {
        var admimConfig = new AdminClientConfig { BootstrapServers = _config.BootstrapServers };

        using var adminClient = new AdminClientBuilder(admimConfig).Build();
        try
        {
            await adminClient.CreateTopicsAsync
            (
                new TopicSpecification[]
                {
                    new TopicSpecification { Name = _topic, NumPartitions = 1, ReplicationFactor = 1 }
                }
            );
            Console.WriteLine($"Topic {_topic} created successfully!");
        }
        catch (CreateTopicsException error)
        {
            if(error.Results[0].Error.Code == ErrorCode.TopicAlreadyExists) { }
            else { Console.WriteLine($"An error occured creating topic {_topic}: {error.Results[0].Error.Reason}"); throw; }
        }
    }

    public void Dispose()
    {
        if (_consumer != null)
        {
            try { _consumer.Close(); }
            catch (Exception error) { Console.WriteLine($"Error during consumer close: {error.Message}"); }
            finally { _consumer.Dispose(); _consumer = null; }
        }
    }
}