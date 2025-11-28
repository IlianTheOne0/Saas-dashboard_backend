using SecurityService = Base.Services.SecurityService;

namespace Base.Entities;

using Kafka.Config;
using Kafka.Entities;

public abstract class Base
{
    private KafkaConsumer? _consumer;
    private KafkaProducer? _producer;
    private CancellationTokenSource? _cancellationToken;

    private void _cleanup()
    {
        _producer?.Dispose();
        _consumer?.Dispose();
        _cancellationToken?.Dispose();

        Console.WriteLine("Service has been stopped and resources have been cleaned up");
    }

    private async Task HandleIncomingMessage(string rawMessage)
    {
        try
        {
            string plainMessage = SecurityService.Decrypt(rawMessage);
            Console.WriteLine($"[DECRYPTED MESSAGE] {plainMessage}");

            string planeResponse = await ProcessMessage(plainMessage);

            if (!string.IsNullOrWhiteSpace(planeResponse) && _producer != null)
            {
                string encryptedResponse = SecurityService.Encrpyt(planeResponse);
                await _producer!.SendMessageAsync(encryptedResponse);
            }
        }
        catch (Exception error) { Console.WriteLine($"Error processing message: {error.Message}"); }
    }

    protected abstract Task<string> ProcessMessage(string message);

    public async Task Run()
    {
        _cancellationToken = new CancellationTokenSource();
        Console.CancelKeyPress += (_, Event) => { Event.Cancel = true; _cancellationToken.Cancel(); };

        try
        {
            Console.WriteLine("Initializing Kafka Producer");
            _producer = new KafkaProducer(KafkaConfig.GetProducerConfig(), KafkaConfig.GetProducerTopic());

            Console.WriteLine("Initializing Kafka Consumer");
            _consumer = new KafkaConsumer(KafkaConfig.GetConsumerConfig(), KafkaConfig.GetConsumerTopic(), HandleIncomingMessage);

            Console.WriteLine("Service Running. Press Ctrl+C to exit");

            await _consumer.Start(_cancellationToken.Token);
        }
        catch (OperationCanceledException) {  Console.WriteLine("Shutdown requested. Exiting..."); }
        catch (Exception error) { Console.WriteLine($"Unexpected error: {error.Message}"); }
        finally { _cleanup(); }
    }
}