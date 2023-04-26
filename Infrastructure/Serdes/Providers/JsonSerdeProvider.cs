using Confluent.Kafka;

namespace KafkaTest.Infrastructure.Serdes.Providers;

public class JsonSerdeProvider<T>: ISerdeProvider<T>
{
    private static Lazy<JsonSerde<T>> _serde = new();

    public ISerializer<T> GetSerializer() => _serde.Value;
    public IDeserializer<T> GetDeserializer() => _serde.Value;
}