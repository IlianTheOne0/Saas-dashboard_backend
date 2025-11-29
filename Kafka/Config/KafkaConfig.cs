
namespace Kafka.Config;

using Confluent.Kafka;

internal class KafkaConsumer
{
    private readonly ConsumerConfig _consumerConfig = new ConsumerConfig
    {
        BootstrapServers = "localhost:9092",
        GroupId = "database",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true,
        SessionTimeoutMs = 6000
    };
    private readonly string _topic = "database-topic";

    public ConsumerConfig GetConsumerConfig() => _consumerConfig;
    public string GetTopic() => _topic;
}

internal class KafkaProducer
{
    private readonly ProducerConfig _producerConfig = new ProducerConfig
    {
        BootstrapServers = "localhost:9092",
        Acks = Acks.All,
        MessageSendMaxRetries = 3,
        RetryBackoffMs = 100,
        LingerMs = 5,
        BatchSize = 32 * 1024
    };
    private readonly string _topic = "database-topic-answers";

    public ProducerConfig GetProducerConfig() => _producerConfig;
    public string GetTopic() => _topic;
}

public class KafkaConfig
{
    private readonly static KafkaConsumer _consumerConfigInstance = new KafkaConsumer();
    private readonly static KafkaProducer _producerConfigInstance = new KafkaProducer();

    public static ConsumerConfig GetConsumerConfig() => _consumerConfigInstance.GetConsumerConfig();
    public static string GetConsumerTopic() => _consumerConfigInstance.GetTopic();
    public static ProducerConfig GetProducerConfig() => _producerConfigInstance.GetProducerConfig();
    public static string GetProducerTopic() => _producerConfigInstance.GetTopic();
}