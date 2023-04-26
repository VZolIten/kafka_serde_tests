using Confluent.Kafka;
using Google.Protobuf;

namespace KafkaTest.Infrastructure.Serdes;

public class ProtobufSerde<T>: ISerializer<T>, IDeserializer<T>
    where T: IMessage<T>
{
    private readonly MessageParser<T> _parser;

    public ProtobufSerde()
    {
        _parser = typeof(T).GetPropertyValue<MessageParser<T>>("Parser")
            ?? throw new ArgumentNullException($"Unable to find parser for {typeof(T).Name}");
    }

    public byte[] Serialize(T data, SerializationContext context)
        => data.ToByteArray();

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        => _parser.ParseFrom(data);
}