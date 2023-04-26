using KafkaTest.Contracts.Entities;

namespace KafkaTest.Producer.Factories;

public class ContactFactory : IContactFactory<Contact>
{
    public Contact GenerateMessage()
        => new(
            Faker.RandomNumber.Next(1, int.MaxValue),
            Faker.Name.FullName(),
            new(
                Faker.Address.City(),
                Faker.Address.StreetName(),
                Faker.RandomNumber.Next(1, 50),
                Faker.Address.ZipCode()
            )
        );
}