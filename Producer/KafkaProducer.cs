using Confluent.Kafka;
using KafkaTest.Infrastructure;
using KafkaTest.Infrastructure.Serdes.Providers;
using KafkaTest.Infrastructure.Settings;
using KafkaTest.Producer.Factories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaTest.Producer;

public sealed class KafkaProducer<TMessage> : BaseKafkaService<ProducerConfig, ProduceException<Null, TMessage>>
    where TMessage: class
{
    private int _counter = 0;
    private readonly IContactFactory<TMessage> _contactFactory;
    private readonly ISerdeProvider<TMessage> _serdeProvider;

    public KafkaProducer(
        ILogger<KafkaProducer<TMessage>> logger,
        IOptions<KafkaSettings> settings, 
        IHostApplicationLifetime appLifetime, 
        IContactFactory<TMessage> contactFactory,
        ISerdeProvider<TMessage> serdeProvider) 
        : base(logger, settings, appLifetime)
    {
        _contactFactory = contactFactory ?? throw new ArgumentNullException(nameof(contactFactory));
        _serdeProvider = serdeProvider ?? throw new ArgumentNullException(nameof(serdeProvider));
    }
    
    protected override async Task StartDoingWork(ProducerConfig config, CancellationToken cancellationToken)
    {
        try
        {
            using var producer = new ProducerBuilder<Null, TMessage>(config)
                .SetValueSerializer(_serdeProvider.GetSerializer())
                .Build();
            _logger.LogInformation("Producing started");
            try
            {
                if (_settings.ProductionTimeoutSec < 0)
                    throw new ArgumentOutOfRangeException(nameof(_settings.ProductionTimeoutSec));
                
                await StartInfiniteProducingAsync(
                    producer,
                    PrepareMessage,
                    TimeSpan.FromSeconds(_settings.ProductionDelaySec),
                    _settings.ProductionTimeoutSec == 0 ? null : TimeSpan.FromSeconds(_settings.ProductionTimeoutSec),
                    cancellationToken);
                
                _logger.LogInformation(">>> Produced {Counter} messages", _counter);
            }
            finally
            {
                producer.Flush(TimeSpan.FromSeconds(10));
            }

            _logger.LogInformation("Producing cancellation has been requested");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Due to the exception a producing will be aborted!");
            throw;
        }

        Message<Null, TMessage> PrepareMessage()
        {
            var newMessage = _contactFactory.GenerateMessage();
            return new() { Value = newMessage };
        }
    }

    protected override ProducerConfig CreateConfig(KafkaSettings settings)
        => new() {
            BootstrapServers = settings.BrokerServer,
        };

    private async Task StartInfiniteProducingAsync<TKey>(
        IProducer<TKey, TMessage> producer,
        Func<Message<TKey, TMessage>> createMessage,
        TimeSpan delay,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        _counter = 0;
        
        if (timeout is not null)
            Cancellation.CancelAfter(timeout.Value);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var message = createMessage();
            var task = producer.ProduceAsync(_settings.Topic, message);
            await task;
            // if (!task.IsCanceled)
            // {
            //     _logger.LogInformation("Produced: {ProducedData}", message.Value);
            // }
            await Task.Delay(delay);
            _counter++;
        }
    }

    protected override void OnStopping()
    {
        base.OnStopping();
        
        _logger.LogInformation("Produced {Count} messages", _counter);
    }
}