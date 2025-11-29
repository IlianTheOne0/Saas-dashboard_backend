
namespace Kafka.Config;

using Confluent.Kafka;

internal class KafkaConsumer
{
    private readonly ConsumerConfig _consumerConfig = new ConsumerConfig
    {
        BootstrapServers = "localhost:9092",
        GroupId = "auth",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true,
        SessionTimeoutMs = 6000
    };
    private readonly List<string> _topics = ["auth-topic", "database-topic-answers"];

    public ConsumerConfig GetConsumerConfig() => _consumerConfig;
    public List<string> GetTopics() => _topics;
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
    private readonly List<string> _topics = ["database-topic", "auth-topic-answers"];

    public ProducerConfig GetProducerConfig() => _producerConfig;
    public List<string> GetTopics() => _topics;
}

public class KafkaConfig
{
    private readonly static KafkaConsumer _consumerConfigInstance = new KafkaConsumer();
    private readonly static KafkaProducer _producerConfigInstance = new KafkaProducer();

    public static ConsumerConfig GetConsumerConfig() => _consumerConfigInstance.GetConsumerConfig();
    public static List<string> GetConsumerTopics() => _consumerConfigInstance.GetTopics();
    public static ProducerConfig GetProducerConfig() => _producerConfigInstance.GetProducerConfig();
    public static List<string> GetProducerTopics() => _producerConfigInstance.GetTopics();
}