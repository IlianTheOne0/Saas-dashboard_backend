using Kafka.Interfaces;

namespace Kafka.Entities;
using Utils;

using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

public class KafkaConsumer : IKafkaConsumer, IDisposable
{
    private const string TAG = "KAFKA-CONSUMER";

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

        Logger.Debug(TAG, $"Building Consumer for Group: {_config.GroupId}");
        _consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
    }

    public Task Start(CancellationToken cancellationToken) =>
    Task.Run
    (
        async () =>
        {
            Logger.Info(TAG, "Starting consumption loop...");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await CreateTopicIfNotExists();

                    _consumer!.Subscribe(_topic);
                    Logger.Info(TAG, $"Subscribed to topic: {_topic}");

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = _consumer!.Consume(cancellationToken);

                            if (result != null)
                            {
                                Logger.Info(TAG, $"[RECV] Offset: {result.TopicPartitionOffset} | Key: {result.Message.Key} | Size: {result.Message.Value?.Length ?? 0} bytes");
                                await _messageHandler(result.Message.Value);
                            }
                        }
                        catch (ConsumeException error)
                        {
                            Logger.Warn(TAG, $"Consume error: {error.Error.Reason} (Code: {error.Error.Code}). Retrying in 1s...");
                            await Task.Delay(1000, cancellationToken);
                        }
                    }
                }
                catch (OperationCanceledException) { Logger.Info(TAG, "Consumer operation canceled."); break; }
                catch (Exception ex) { Logger.Fatal(TAG, "FATAL ERROR in Consumer Loop. Retrying in 5s...", ex); await Task.Delay(5000, cancellationToken); }
            }
            Logger.Info(TAG, "Closing consumer connection...");
            _consumer!.Close();
        },
        cancellationToken
    );

    private async Task CreateTopicIfNotExists()
    {
        Logger.Debug(TAG, $"Checking topic existence: {_topic}");
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
            Logger.Info(TAG, $"Topic {_topic} created successfully!");
        }
        catch (CreateTopicsException error)
        {
            if (error.Results[0].Error.Code == ErrorCode.TopicAlreadyExists) { Logger.Debug(TAG, $"Topic {_topic} already exists."); }
            else { Logger.Error(TAG, $"Error creating topic {_topic}", new Exception(error.Results[0].Error.Reason)); throw; }
        }
    }

    public void Dispose()
    {
        if (_consumer != null)
        {
            try { Logger.Debug(TAG, "Disposing Consumer..."); _consumer.Close(); }
            catch (Exception error) { Logger.Error(TAG, "Error during consumer close", error); }
            finally { _consumer.Dispose(); _consumer = null; }
        }
    }

    public string GetTopic() => _topic;
}