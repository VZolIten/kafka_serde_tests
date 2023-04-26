using System.Text.Json.Serialization.Metadata;
using Confluent.Kafka;

namespace KafkaTest.Infrastructure.Serdes.Providers;

public class JsonAotSerdeProvider<T>: ISerdeProvider<T>
{
    private readonly Lazy<JsonAotSerde<T, JsonTypeInfo<T>>> _serde;

    public JsonAotSerdeProvider(JsonTypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        _serde = new(() => new JsonAotSerde<T, JsonTypeInfo<T>>(typeInfo));
    }

    public ISerializer<T> GetSerializer() => _serde.Value;
    public IDeserializer<T> GetDeserializer() => _serde.Value;
}