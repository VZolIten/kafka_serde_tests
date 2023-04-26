using KafkaTest.Contracts.ProtoEntities.Generated;
using KafkaTest.Infrastructure.Extensions;
using KafkaTest.Infrastructure.Serdes.Providers;
using KafkaTest.Infrastructure.Settings;
using KafkaTest.Producer;
using KafkaTest.Producer.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Contact = KafkaTest.Contracts.Entities.Contact;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var kafkaSection = hostContext.Configuration.GetRequiredSection(KafkaSettings.SECTION_NAME); 
        services.Configure<KafkaSettings>(kafkaSection);
        
        var serdeType = kafkaSection.GetValue<SerdeType>(nameof(KafkaSettings.Serde));

        if (serdeType is SerdeType.Protobuf)
        {
            services.AddSingleton<ISerdeProvider<ContactProto>, ProtobufSerdeProvider<ContactProto>>();
            services.AddSingleton<IContactFactory<ContactProto>, ContactProtoFactory>();
            services.AddHostedService<KafkaProducer<ContactProto>>();
        }
        else
        {
            services.ConfigureJsonSerdeProviderFor<Contact>(serdeType);
            services.AddSingleton<IContactFactory<Contact>, ContactFactory>();
            services.AddHostedService<KafkaProducer<Contact>>();
        }
    })
    .Build();

host.Run();