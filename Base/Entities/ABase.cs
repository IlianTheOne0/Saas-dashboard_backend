using SecurityService = Base.Services.SecurityService;

namespace Base.Entities;

using Kafka.Config;
using Kafka.Entities;
using System.Diagnostics;
using Utils;

public abstract class Base
{
    private const string TAG = "BASE-SERVICE";

    private List<KafkaConsumer>? _consumers;
    private Dictionary<string, KafkaProducer>? _producers;
    private CancellationTokenSource? _cancellationToken;

    private void _cleanup()
    {
        Logger.Info(TAG, "Starting resource cleanup...");
        if (_producers != null)
        {
            foreach (var producer in _producers.Values)
            {
                Logger.Info(TAG, $"Disposing producer for topic #{producer.GetTopic()}...");
                producer.Dispose();
            }
        }
        if (_consumers != null)
        {
            foreach (var consumer in _consumers)
            {
                Logger.Info(TAG, $"Disposing producer for topic #{consumer.GetTopic()}...");
                consumer.Dispose();
            }
        }
        _cancellationToken?.Dispose();
        Logger.Info(TAG, "Resources cleaned up.");
    }

    private async Task HandleIncomingMessage(string rawMessage)
    {
        var stopWatch = Stopwatch.StartNew();
        Logger.Info(TAG, $">>> New Message Received. Length: {rawMessage.Length} chars.");

        try
        {
            Logger.Debug(TAG, "Decrypting message...");
            string plainMessage = SecurityService.Decrypt(rawMessage);
            Logger.Debug(TAG, "Decryption successful.");
            
            Logger.Info(TAG, "Processing message logic...");
            string planeResponse = await ProcessMessage(plainMessage);

            if (!string.IsNullOrWhiteSpace(planeResponse) && _producers != null)
            {
                Logger.Debug(TAG, "Encrypting response...");
                string encryptedResponse = SecurityService.Encrpyt(planeResponse);

                Logger.Info(TAG, "Sending response to Kafka...");
                if (_producers.ContainsKey("database")) { await _producers["database"].SendMessageAsync(encryptedResponse); }
                Logger.Info(TAG, "Response sent successfully.");
            }
            else { Logger.Warn(TAG, "No response generated or Producer is null. Skipping reply."); }
        }
        catch (Exception error) { Logger.Error(TAG, "Error processing message pipeline", error); }
        finally { stopWatch.Stop(); Logger.Info(TAG, $"<<< Cycle finished in {stopWatch.ElapsedMilliseconds}ms."); }
    }

    protected abstract Task<string> ProcessMessage(string message);

    protected async Task produceMessage(string topic, string message)
    {
        if (_producers == null) { throw new InvalidOperationException("Producers not initialized"); }

        var producerEntry = _producers.FirstOrDefault(producer => producer.Key.Contains(topic));

        if (producerEntry.Value != null)
        {
            var producer = producerEntry.Value;

            string encryptedMessage = SecurityService.Encrpyt(message);
            await producer.SendMessageAsync(encryptedMessage);
        }
        else
        {
            string availableTopics = string.Join(", ", _producers.Keys);
            throw new InvalidOperationException($"No producer found containing '{topic}'. Available topics: [{availableTopics}]");
        }
    }

    public async Task Run()
    {
        _cancellationToken = new CancellationTokenSource();
        Console.CancelKeyPress += (_, Event) => { Logger.Warn(TAG, "Cancel key pressed. Initiating shutdown..."); Event.Cancel = true; _cancellationToken.Cancel(); };

        try
        {
            Logger.Info(TAG, "----------------------------------------");
            Logger.Info(TAG, "Initializing Kafka Producer...");
            _producers = new Dictionary<string, KafkaProducer>();
            List<string> producerTopics = KafkaConfig.GetProducerTopics();

            foreach (var topic in producerTopics)
            {
                Logger.Info(TAG, $"Creating producer for topic: {topic}...");
                _producers.Add(topic, new KafkaProducer(KafkaConfig.GetProducerConfig(), topic));
            }

            Logger.Info(TAG, "Initializing Kafka Consumer...");
            _consumers = new List<KafkaConsumer>();
            List<string> consumerTopics = KafkaConfig.GetConsumerTopics();
            foreach (var topic in consumerTopics)
            {
                Logger.Info(TAG, $"Creating consumer for topic: {topic}...");
                _consumers!.Add(new KafkaConsumer(KafkaConfig.GetConsumerConfig(), topic, HandleIncomingMessage));
            }

            Logger.Info(TAG, "Service Running. Press Ctrl+C to exit.");
            Logger.Info(TAG, "----------------------------------------");

            var tasks = _consumers.Select(consumer => consumer.Start(_cancellationToken.Token));
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException) { Logger.Info(TAG, "Shutdown requested. Exiting loop..."); }
        catch (Exception error) { Logger.Fatal(TAG, "Unexpected fatal error", error); }
        finally { _cleanup(); }
    }
}