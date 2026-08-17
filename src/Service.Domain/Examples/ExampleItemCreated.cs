using Service.Domain.Abstractions;

namespace Service.Domain.Examples;

public sealed record ExampleItemCreated(Guid ExampleItemId, DateTimeOffset OccurredAt) : IDomainEvent;
