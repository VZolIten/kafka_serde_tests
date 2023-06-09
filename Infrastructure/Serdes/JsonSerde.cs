﻿using System.Text.Json;
using Confluent.Kafka;

namespace KafkaTest.Infrastructure.Serdes;

public class JsonSerde<T> : ISerializer<T>, IDeserializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        using var ms = new MemoryStream();
        string jsonString = JsonSerializer.Serialize(data);
        var writer = new StreamWriter(ms);

        writer.Write(jsonString);
        writer.Flush();
        ms.Position = 0;

        return ms.ToArray();
    }

    T IDeserializer<T>.Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull) throw new Exception("Value is null");
        
        return JsonSerializer.Deserialize<T>(data.ToArray())!;
    }
}