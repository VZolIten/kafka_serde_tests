namespace KafkaTest.Contracts.Entities;

public record Contact (
    int Id,
    string Name,
    Address Address
);

public record Address (
    string City,
    string Street,
    int Building,
    string ZipCode
);