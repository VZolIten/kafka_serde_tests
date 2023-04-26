using Confluent.Kafka;
using KafkaTest.Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaTest.Infrastructure;

public abstract class BaseKafkaService<TConfig, TSpecificException>: IHostedService
    where TConfig: ClientConfig
    where TSpecificException: Exception 
{
    protected readonly ILogger _logger;
    protected readonly KafkaSettings _settings;
    protected readonly CancellationTokenSource Cancellation = new();
    private CancellationToken ApplicationCancellationToken => Cancellation.Token;
    private IHostApplicationLifetime _appLifetime;
    
    protected BaseKafkaService(
        ILogger logger,
        IOptions<KafkaSettings> settings,
        IHostApplicationLifetime appLifetime)
    {
        ArgumentNullException.ThrowIfNull(appLifetime);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var config = CreateConfig(_settings);

        var _ = Task.Run(async () =>
        {
            const int TRIES_MAX_COUNT = 10;
            int tryNumber = 1;
            do
            {
                try
                {
                    await LetKafkaServiceStart(ApplicationCancellationToken);
                    await StartDoingWork(config, ApplicationCancellationToken);
                }
                catch (TaskCanceledException) // From Delay
                {
                    _logger.LogInformation("Task cancellation Requested");
                }
                catch (TSpecificException)
                {
                    tryNumber++;
                }
            } while (tryNumber < TRIES_MAX_COUNT && !ApplicationCancellationToken.IsCancellationRequested);
            _appLifetime.StopApplication();
        }, cancellationToken);
        
        return Task.CompletedTask;
    }

    protected abstract TConfig CreateConfig(KafkaSettings settings);
    protected abstract Task StartDoingWork(TConfig config, CancellationToken cancellationToken);

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        Cancellation.Cancel();
        return Task.CompletedTask;
    }
    
    protected virtual void OnStarted()
    {
        _logger.LogInformation("Started");
    }

    protected virtual void OnStopping()
    {
        _logger.LogInformation("Stopping...");
    }

    protected virtual void OnStopped()
    {
        _logger.LogInformation("Stopped");
    }
    
    private static Task LetKafkaServiceStart(CancellationToken cancellationToken = default)
        => Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
}