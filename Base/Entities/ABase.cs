using SecurityService = Base.Services.SecurityService;

namespace Base.Entities;

using Kafka.Config;
using Kafka.Entities;
using System.Diagnostics;
using Utils;

public abstract class Base
{
    private const string TAG = "BASE-SERVICE";

    private KafkaConsumer? _consumer;
    private KafkaProducer? _producer;
    private CancellationTokenSource? _cancellationToken;

    private void _cleanup()
    {
        Logger.Info(TAG, "Starting resource cleanup...");
        _producer?.Dispose();
        _consumer?.Dispose();
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

            if (!string.IsNullOrWhiteSpace(planeResponse) && _producer != null)
            {
                Logger.Debug(TAG, "Encrypting response...");
                string encryptedResponse = SecurityService.Encrpyt(planeResponse);

                Logger.Info(TAG, "Sending response to Kafka...");
                await _producer.SendMessage(encryptedResponse);
                Logger.Info(TAG, "Response sent successfully.");
            }
            else { Logger.Warn(TAG, "No response generated or Producer is null. Skipping reply."); }
        }
        catch (Exception error) { Logger.Error(TAG, "Error processing message pipeline", error); }
        finally { stopWatch.Stop(); Logger.Info(TAG, $"<<< Cycle finished in {stopWatch.ElapsedMilliseconds}ms."); }
    }

    protected abstract Task<string> ProcessMessage(string message);

    public async Task Run()
    {
        _cancellationToken = new CancellationTokenSource();
        Console.CancelKeyPress += (_, Event) => { Logger.Warn(TAG, "Cancel key pressed. Initiating shutdown..."); Event.Cancel = true; _cancellationToken.Cancel(); };

        try
        {
            Logger.Info(TAG, "----------------------------------------");
            Logger.Info(TAG, "Initializing Kafka Producer...");
            _producer = new KafkaProducer(KafkaConfig.GetProducerConfig(), KafkaConfig.GetProducerTopic());

            Logger.Info(TAG, "Initializing Kafka Consumer...");
            _consumer = new KafkaConsumer(KafkaConfig.GetConsumerConfig(), KafkaConfig.GetConsumerTopic(), HandleIncomingMessage);

            Logger.Info(TAG, "Service Running. Press Ctrl+C to exit.");
            Logger.Info(TAG, "----------------------------------------");

            await _consumer.Start(_cancellationToken.Token);
        }
        catch (OperationCanceledException) { Logger.Info(TAG, "Shutdown requested. Exiting loop..."); }
        catch (Exception error) { Logger.Fatal(TAG, "Unexpected fatal error", error); }
        finally { _cleanup(); }
    }
}