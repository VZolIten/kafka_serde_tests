using Confluent.Kafka;

namespace KafkaTest.Infrastructure.Serdes.Providers;

public interface ISerdeProvider<T>
{
    ISerializer<T> GetSerializer();
    IDeserializer<T> GetDeserializer();
}