using System.Text.Json.Serialization.Metadata;
using Confluent.Kafka;
using KafkaTest.Consumer.Processors;
using KafkaTest.Infrastructure;
using KafkaTest.Infrastructure.Serdes;
using KafkaTest.Infrastructure.Serdes.Providers;
using KafkaTest.Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaTest.Consumer;

public sealed class KafkaConsumer<TKey, TMessage> : BaseKafkaService<ConsumerConfig, ConsumeException>
    where TMessage: class
{
    private IConsumer<TKey, TMessage>? _currentConsumer;
    private readonly IContactProcessor<TMessage> _contactProcessor;
    private readonly ISerdeProvider<TMessage> _serdeProvider;

    public KafkaConsumer(
        ILogger<KafkaConsumer<TKey, TMessage>> logger,
        IOptions<KafkaSettings> settings,
        IHostApplicationLifetime appLifetime, 
        IContactProcessor<TMessage> contactProcessor,
        ISerdeProvider<TMessage> serdeProvider)
        : base(logger, settings, appLifetime)
    {
        _contactProcessor = contactProcessor ?? throw new ArgumentNullException(nameof(contactProcessor));
        _serdeProvider = serdeProvider ?? throw new ArgumentNullException(nameof(serdeProvider));
    }

    protected override async Task StartDoingWork(ConsumerConfig config, CancellationToken cancellationToken)
    {
        try
        {
            using var consumer = new ConsumerBuilder<TKey, TMessage>(config)
                .SetPartitionsRevokedHandler((consumer, list) => consumer.Commit(list))
                .SetValueDeserializer(_serdeProvider.GetDeserializer())
                .Build();
            _currentConsumer = consumer;
            consumer.Subscribe(_settings.Topic);

            _logger.LogInformation("Consumption started");
            try
            {
                await StartInfiniteConsumptionAsync(consumer, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                CommitOffsets(consumer);
                _logger.LogInformation("Consumption cancellation has been requested");
            }
            finally
            {
                _currentConsumer = null;
                CloseConsumer(consumer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Due to the exception a consumption will be aborted!");
            throw;
        }
    }

    protected override ConsumerConfig CreateConfig(KafkaSettings settings)
        => new() {
            BootstrapServers = settings.BrokerServer, 
            GroupId = "Group#1",
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
    
    protected override void OnStopping()
    {
        if (_currentConsumer is null) return;
        
        CommitOffsets(_currentConsumer);
        
        base.OnStopping();
    }

    private void CloseConsumer(IConsumer<TKey, TMessage> consumer)
    {
        try
        {
            consumer.Close();
        }
        catch (KafkaException ex)
        {
            _logger.LogError(ex, "Error during close of a consumer");
        }
    }

    private async Task StartInfiniteConsumptionAsync(IConsumer<TKey, TMessage> consumer, CancellationToken cancellationToken)
    {
        var commitFrequency = _settings.ConsumersCommitEach;
        
        var processedCount = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = consumer.Consume(cancellationToken);
            await ProcessRecordAsync(result, cancellationToken);
            processedCount++;

            if (ShouldCommitOffsets(processedCount))
            {
                CommitOffsets(consumer, result);
            }
        }

        bool ShouldCommitOffsets(int processed) => processed % commitFrequency == 0;
    }

    private void CommitOffsets(IConsumer<TKey, TMessage> consumer, ConsumeResult<TKey, TMessage>? result = null)
    {
        try
        {
            if (result is not null)
            {
                _logger.LogInformation("Commit offset {Offset}", result.Offset);
                consumer.Commit(result);
            }
            else
            {
                _logger.LogInformation("Commit offset {Offset}", consumer.Committed(TimeSpan.FromSeconds(1)));
                consumer.Commit();
            }
        }
        catch (KafkaException ex)
        {
            _logger.LogError(ex, "Unable to commit offset");
        }
    }

    private Task ProcessRecordAsync(ConsumeResult<TKey, TMessage> record, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consumed: {@ConsumedData}, offset: {Offset}", record.Message.Value, record.Offset.Value);

        return _contactProcessor.ProcessAsync(record.Message.Value, cancellationToken);
    }
}