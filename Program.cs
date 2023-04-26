using KafkaTest.KafkaTest.Consumer;
using KafkaTest.KafkaTest.Consumer.Processors;
using KafkaTest.KafkaTest.Contracts.Entities;
using KafkaTest.KafkaTest.Contracts.Settings;
using KafkaTest.KafkaTest.Producer;
using KafkaTest.KafkaTest.Producer.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IMessageFactory<MyMessage>, MyMessageFactory>();
        services.AddScoped<IMessageProcessor<MyMessage>, MyMessageProcessor>();
        
        services.AddHostedService<KafkaProducer<MyMessage>>();
        services.AddHostedService<KafkaConsumer<MyMessage>>();

        services.Configure<KafkaSettings>(
            hostContext.Configuration.GetSection("Kafka"));
    })
    .Build();

host.Run();