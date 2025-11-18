using Kafka.Interfaces;

namespace Kafka.Entities;

using System.Threading;
using Confluent.Kafka;

public class Consumer : IConsumer, IDisposable
{
    private IConsumer<Ignore, string>? _consumer;

    private ConsumerConfig _config = null!;
    private string _topic = null!;

    private readonly Action<string> _messageHandler;

    public Consumer(ConsumerConfig config, string topic, Action<string> messageHandler)
    {
        if (config == null) { throw new ArgumentNullException("'config' argument can not be null!"); }
        if (topic == null) { throw new ArgumentNullException("'topic' argument can not be null!"); }
        if (messageHandler == null) { throw new ArgumentNullException("'messageHandler' argument can not be null!"); }

        _config = config;
        _topic = topic;
        _messageHandler = messageHandler;

        _consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
        _consumer.Subscribe(_topic);
    }

    public Task Start(CancellationToken cancellationToken) => Task.Run
    (
        () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer!.Consume(cancellationToken);

                    if (result != null)
                    {
                        _consumer.Commit(result);
                        _messageHandler(result.Message.Value);
                    }
                }
                catch (OperationCanceledException) { }
                finally { _consumer!.Close(); }
            }
        }, cancellationToken
    );

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