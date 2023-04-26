using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Confluent.Kafka;

namespace KafkaTest.Infrastructure.Serdes;

public class JsonAotSerde<T, TContext> : ISerializer<T>, IDeserializer<T>
    where TContext: JsonTypeInfo<T>
{
    private readonly TContext _context;

    public JsonAotSerde(TContext context)
    {
        _context = context;
    }

    public byte[] Serialize(T data, SerializationContext context)
         => JsonSerializer.SerializeToUtf8Bytes(data, _context);

    T IDeserializer<T>.Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull) throw new Exception("Value is null");
        
        return JsonSerializer.Deserialize(data.ToArray(), _context)!;
    }
}