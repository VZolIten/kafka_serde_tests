using System.Text.Json.Serialization;

namespace KafkaTest.Contracts.Entities;

[JsonSerializable(typeof(Contact))]
public partial class ContactContext: JsonSerializerContext 
{
}