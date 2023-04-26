namespace KafkaTest.Infrastructure.Settings;

public class KafkaSettings
{
    public const string SECTION_NAME = "Kafka";
    
    public string Topic { get; set; } = null!;
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public int ConsumersCommitEach { get; set; }
    public int ProductionDelaySec { get; set; }
    public int ProductionTimeoutSec { get; set; }
    public SerdeType Serde { get; set; }

    public string BrokerServer => $"{Host}:{Port}";
}

public enum SerdeType
{
    Json,
    JsonAot,
    Protobuf,
}