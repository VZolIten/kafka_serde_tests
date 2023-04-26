using Confluent.Kafka;
using KafkaTest.Consumer;
using KafkaTest.Consumer.Processors;
using KafkaTest.Contracts.Entities;
using KafkaTest.Contracts.ProtoEntities.Generated;
using KafkaTest.Infrastructure.Extensions;
using KafkaTest.Infrastructure.Serdes.Providers;
using KafkaTest.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var kafkaSection = hostContext.Configuration.GetRequiredSection(KafkaSettings.SECTION_NAME);
        services.Configure<KafkaSettings>(kafkaSection); 

        var serdeType = kafkaSection.GetValue<SerdeType>(nameof(KafkaSettings.Serde));

        if (serdeType is SerdeType.Protobuf)
        {
            services.AddSingleton<ISerdeProvider<ContactProto>, ProtobufSerdeProvider<ContactProto>>();
            services.AddSingleton<IContactProcessor<ContactProto>, ContactProtoProcessor>();
            services.AddHostedService<KafkaConsumer<Null, ContactProto>>();
        }
        else
        {
            services.ConfigureJsonSerdeProviderFor<Contact>(serdeType);
            services.AddSingleton<IContactProcessor<Contact>, ContactProcessor>();
            services.AddHostedService<KafkaConsumer<Null, Contact>>();
        }
    })
    .Build()
    .Run();