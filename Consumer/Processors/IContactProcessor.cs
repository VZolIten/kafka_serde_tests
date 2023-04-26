namespace KafkaTest.Consumer.Processors;

public interface IContactProcessor<TMessage>
{
    Task ProcessAsync(TMessage message, CancellationToken cancellationToken = default);
}