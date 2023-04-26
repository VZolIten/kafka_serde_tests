using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using KafkaTest.Infrastructure.Serdes.Providers;
using KafkaTest.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaTest.Infrastructure.Extensions;

public static class SerdeConfigurationExtensions
{
    public static IServiceCollection ConfigureJsonSerdeProviderFor<T>(this IServiceCollection services, SerdeType serdeType)
    {
        switch (serdeType)
        {
            case SerdeType.Json:
                return services.AddSingleton<ISerdeProvider<T>, JsonSerdeProvider<T>>();
            
            case SerdeType.JsonAot:
                return services
                    .AddSingleton<ISerdeProvider<T>, JsonAotSerdeProvider<T>>()
                    .AddSingleton(GetTypeInfo<T>());
            
            default:
                throw new NotImplementedException(serdeType.ToString());
        };
    }

    private static JsonTypeInfo<T> GetTypeInfo<T>()
    {
        var context = typeof(T).Assembly.GetTypes()
            .Where(type => type.BaseType == typeof(JsonSerializerContext))
            .Select(type =>
            {
                var attribute = type.GetCustomAttribute<JsonSerializableAttribute>();
                if (attribute is null) return null;

                return type.GetPropertyValue<JsonSerializerContext>(attribute.TypeInfoPropertyName ?? "Default");
            })
            .FirstOrDefault(x => x is not null);

        var typeName = typeof(T).Name;

        return context?.GetType().GetPropertyValue<JsonTypeInfo<T>>(typeName, context)
            ?? throw new ArgumentNullException($"Unable to find type info for type {typeName}");
    }
}