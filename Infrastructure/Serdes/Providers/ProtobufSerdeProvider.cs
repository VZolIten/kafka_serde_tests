using Confluent.Kafka;
using Google.Protobuf;

namespace KafkaTest.Infrastructure.Serdes.Providers;

public class ProtobufSerdeProvider<T>: ISerdeProvider<T>
    where T: IMessage<T>
{
    private readonly ProtobufSerde<T> _serde = new ();

    public ISerializer<T> GetSerializer() => _serde;
    public IDeserializer<T> GetDeserializer() => _serde;
}