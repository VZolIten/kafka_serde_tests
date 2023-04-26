
using KafkaTest.Contracts.ProtoEntities.Generated;

namespace KafkaTest.Producer.Factories;

public class ContactProtoFactory : IContactFactory<ContactProto>
{
    public ContactProto GenerateMessage()
        => new()
        {
            Id = Faker.RandomNumber.Next(1, int.MaxValue),
            Name = Faker.Name.FullName(),
            Address = new()
            {
                City = Faker.Address.City(),
                Street = Faker.Address.StreetName(),
                Building = Faker.RandomNumber.Next(1, 50),
                ZipCode = Faker.Address.ZipCode(),
            },
        };
}