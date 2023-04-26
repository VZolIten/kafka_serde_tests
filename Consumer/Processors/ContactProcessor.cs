using KafkaTest.Contracts.Entities;
using Microsoft.Extensions.Logging;

namespace KafkaTest.Consumer.Processors;

public class ContactProcessor: IContactProcessor<Contact>
{
    private readonly ILogger _logger;

    public ContactProcessor(ILogger<ContactProcessor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ProcessAsync(Contact message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Message id is {@Id}", message.Id);
        return Task.CompletedTask;
    }
}