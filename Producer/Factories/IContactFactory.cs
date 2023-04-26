namespace KafkaTest.Producer.Factories;

public interface IContactFactory<TMessage>
{
    TMessage GenerateMessage();
}