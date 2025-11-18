namespace Kafka.Config;

using Confluent.Kafka;

internal class CConsumer
{
    private readonly ConsumerConfig _consumerConfig = new ConsumerConfig
    {
        BootstrapServers = "localhost:9092",
        GroupId = "saas_dashboard-react",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
        SessionTimeoutMs = 6000
    };
    private readonly string _topic = "default-topic_answer";

    public ConsumerConfig GetConsumerConfig() => _consumerConfig;
    public string GetTopic() => _topic;
}

internal class CProducer
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
    private readonly string _topic = "default-topic";

    public ProducerConfig GetProducerConfig() => _producerConfig;
    public string GetTopic() => _topic;
}

public class KafkaConfig
{
    private readonly static CConsumer _consumerConfigInstance = new CConsumer();
    private readonly static CProducer _producerConfigInstance = new CProducer();

    public static ConsumerConfig GetConsumerConfig() => _consumerConfigInstance.GetConsumerConfig();
    public static string GetConsumerTopic() => _consumerConfigInstance.GetTopic();
    public static ProducerConfig GetProducerConfig() => _producerConfigInstance.GetProducerConfig();
    public static string GetProducerTopic() => _producerConfigInstance.GetTopic();
}