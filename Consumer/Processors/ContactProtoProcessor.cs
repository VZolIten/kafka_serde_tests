using KafkaTest.Contracts.ProtoEntities.Generated;
using Microsoft.Extensions.Logging;

namespace KafkaTest.Consumer.Processors;

public class ContactProtoProcessor: IContactProcessor<ContactProto>
{
    private readonly ILogger _logger;

    public ContactProtoProcessor(ILogger<ContactProtoProcessor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public Task ProcessAsync(ContactProto message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Message id is {@Id}", message.Id);
        return Task.CompletedTask;
    }
}